using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Application.Common.Exceptions;
using CrmApp.Domain.DTO;
using CrmApp.Domain.Entities;
using CrmApp.Domain.Enums;
using CrmApp.Domain.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Transactions;
using static System.Net.WebRequestMethods;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CrmApp.Infrastructure.Identity
{
    public sealed class UsersManager
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roles;
        private readonly IUsersRepository _usersRepository;
        private readonly IHttpContextAccessor _http;
        private readonly IEmailSender _emailSender;
        private readonly IMemoryCache _cache;
        private readonly string _appUrl;

        public UsersManager(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IUsersRepository usersRepository,
            RoleManager<IdentityRole> roles,
            IHttpContextAccessor httpContextAccessor,
            IEmailSender emailSender,
            IMemoryCache cache,
            IConfiguration cfg)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _usersRepository = usersRepository;
            _http = httpContextAccessor;
            _roles = roles;
            _cache = cache;
            _emailSender = emailSender; 
            _appUrl = cfg["Frontend:BaseUrl"]!.TrimEnd('/');
        }

        public async Task<ResultDTO<UsersDTO>> UpdateAsync(UsersDTO user, bool updateClaims = false, CancellationToken ct = default)
        {
            var userProfile = await _usersRepository.GetAsync(user.Id, ct);
            if (userProfile == null) 
            {
                return new ErrorResultDTO<UsersDTO>(["Nie znaleziono profilu użytkownika w bazie"]);
            }

            var appUser = await _userManager.FindByIdAsync(userProfile!.UserId ?? "");

            userProfile.FirstName = user.FirstName;
            userProfile.LastName = user.LastName;
            userProfile.DisplayName = user.DisplayName;
            userProfile.AlternativeEmails = user.AlternativeEmails != null ? string.Join(",", user.AlternativeEmails) : string.Empty;
            userProfile.CalendarProvider = user.CalendarProvider;
            userProfile.CalendarId = user.CalendarId;

            if (appUser!.Email != user.Email)
            {
                bool isUserProfileExists = await _usersRepository.ProfileExistsAsync(user.Email!, ct);
                if (isUserProfileExists)
                    return new ErrorResultDTO<UsersDTO>(["Użytkownik o takim adresie e-mail istnieje już w systemie"]);
            }

            appUser!.Email = user.Email;
            appUser!.PhoneNumber = user.PhoneNumber;

            if (updateClaims)
            {
                await UpdateUserRoles(appUser, user.UserConfiguration.Roles ?? []);
                await UpdateUserClaims(appUser, user.UserConfiguration.Claims != null ? [.. user.UserConfiguration.Claims.Select(x => x.Key)] : []);
            }

            await _userManager.UpdateAsync(appUser);
            await _usersRepository.UpdateAsync(userProfile, ct);
            await _usersRepository.SaveChangesAsync(ct);
            return new SuccessResultDTO<UsersDTO>(user.Id.ToString(), []) { Data = user };
        } 


        public async Task UpdateUserClaims(ApplicationUser appUser, List<string> claims)
        {
            var userClaimsInDB = await _userManager.GetClaimsAsync(appUser);
            var claimsToRemove = userClaimsInDB.Where(x => !claims.Contains(x.Value)).ToList();
            var claimsToAdd = claims.Where(x => !userClaimsInDB.Select(c => c.Value).Contains(x))
                .Select(x => new Claim(AppClaimTypes.Permission, x)).ToList();
               
            if (claimsToAdd.Count != 0) await _userManager.AddClaimsAsync(appUser, claimsToAdd);
            if (claimsToRemove.Count != 0) await _userManager.RemoveClaimsAsync(appUser, claimsToRemove);

            _cache.Remove($"perm:{appUser.Id}");
        }

        public async Task UpdateUserRoles(ApplicationUser appUser, List<string> roles)
        {
            var userRolesInDb = await _userManager.GetRolesAsync(appUser);
            var rolesToRemove = userRolesInDb.Where(x => !roles.Contains(x)).ToList();
            var rolesToAdd = roles.Where(x => !rolesToRemove.Contains(x)).ToList();

            if (rolesToAdd.Count != 0)
            {
                foreach(var role in rolesToAdd)
                {
                    var appRole = await _roles.FindByNameAsync(role) ?? throw new KeyNotFoundException($"Nie znaleziono roli {role} w systemie");
                    //var rolesClaims = await _roles.GetClaimsAsync(appRole);
                    //await _userManager.AddClaimsAsync(appUser, rolesClaims);
                    await _userManager.AddToRoleAsync(appUser, role);
                }
            }
            if (rolesToRemove.Count != 0) await _userManager.RemoveFromRolesAsync(appUser, rolesToRemove);
            
            _cache.Remove($"perm:{appUser.Id}");
        }

        public async Task<ResultDTO<object>> Deactivate(int id, CancellationToken ct = default)
        {
            var userProfile = await _usersRepository.GetAsync(id, ct);
            if (userProfile == null)
                return new ErrorResultDTO<object>(["Nie znaleziono profilu użytkownika w bazie"]);
            
            if (string.IsNullOrWhiteSpace(userProfile.UserId))
                return new ErrorResultDTO<object>(["Użytkownik nie posiada konta w aplikacji"]);

            var appUser = await _userManager.FindByIdAsync(userProfile.UserId) ?? throw new KeyNotFoundException("Użytkownik nie posiada konta w aplikacji");
            await _userManager.SetLockoutEnabledAsync(appUser, true);
            await _usersRepository.Deactivate(userProfile);
            await _usersRepository.SaveChangesAsync(ct);

            return new SuccessResultDTO<object>(id.ToString(), []);
        }

        public async Task<ResultDTO<UsersDTO>> AddAsync(RegisterDTO model, bool updateClaims = false, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(model.UserDTO.Email)) return new ErrorResultDTO<UsersDTO>(["Podaj adres e-mail do rejestracji"]);
            bool isUserProfileExists = await _usersRepository.ProfileExistsAsync(model.UserDTO.Email, ct);
            if (isUserProfileExists)
                return new ErrorResultDTO<UsersDTO>(["Użytkownik o takim adresie e-mail istnieje już w systemie"]);

            using var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted },
                TransactionScopeAsyncFlowOption.Enabled);

            var user = new ApplicationUser
            {
                UserName = model.UserDTO.Email,
                Email = model.UserDTO.Email,
                PhoneNumber = model.UserDTO.PhoneNumber
            };

            var identityResult = await _userManager.CreateAsync(user, model.Password);
            if (!identityResult.Succeeded)
            {
                var errs = identityResult.Errors.Select(e => e.Description).ToArray();
                return new ErrorResultDTO<UsersDTO>(errs);
            }

            if (updateClaims)
            {
                await UpdateUserClaims(user, model.UserDTO.UserConfiguration.Claims != null ? [.. model.UserDTO.UserConfiguration.Claims.Select(x => x.Key)] : []);
                await UpdateUserRoles(user, model.UserDTO.UserConfiguration.Roles ?? []);
            }

            var profile = new UsersProfiles
            {
                UserId = user.Id,
                DisplayName = model.UserDTO.DisplayName,
                FirstName = model.UserDTO.FirstName,
                LastName = model.UserDTO.LastName,
                ForeignSystemOperatorId = model.UserDTO.ForeignSystemOperatorId,
                ForeignSystemType = model.UserDTO.ForeignSystemType,
                AlternativeEmails = model.UserDTO.AlternativeEmails != null ? string.Join(",", model.UserDTO.AlternativeEmails) : string.Empty,
                CalendarProvider = model.UserDTO.CalendarProvider,
                CalendarId = model.UserDTO.CalendarId
            };

            try
            {
                await _usersRepository.AddAsync(profile, ct);
                await _usersRepository.SaveChangesAsync(ct);
                await SendAccountCreatedEmail(user, profile, ct);

                scope.Complete();
                return new SuccessResultDTO<UsersDTO>
                {
                    Succeeded = true,
                    ObjectId = user.Id,
                    Data = GetUserDTO(profile, user)
                };
            }
            catch (Exception ex)
            {
                if (profile.Id > 0)
                {
                    await _usersRepository.Remove(profile);
                    await _usersRepository.SaveChangesAsync(ct);
                }
                await _userManager.DeleteAsync(user);
                return new ErrorResultDTO<UsersDTO>([ex.Message]);
            }
        }

        public async Task<ResultDTO<UsersDTO>> Login(LoginDTO login, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(login.Email);
            if (user is null) return new ErrorResultDTO<UsersDTO>(["Nie znaleziono użytkownika w systemie o podanym adresie e-mail"]);

            UsersProfiles? userProfile = await _usersRepository.GetByUserIdAsync(user.Id, ct);
            if (userProfile == null || userProfile.IsDeleted) return new ErrorResultDTO<UsersDTO>(["Konto zostało dezaktywowane"]);

            var res = await _signInManager.PasswordSignInAsync(
                user, login.Password, isPersistent: login.RememberMe, lockoutOnFailure: true);

            if (res.Succeeded)
                return new SuccessResultDTO<UsersDTO>(user.Id)
                {
                    Data = GetUserDTO(
                        userProfile ?? new UsersProfiles(),
                        user,
                        _usersRepository.GetUserConfiguration(user.Id))
                };

            if (res.IsLockedOut)
                return new ErrorResultDTO<UsersDTO>(["Konto jest zablokowane"]);

            if (res.IsNotAllowed)
                return new ErrorResultDTO<UsersDTO>(["Użytkownik nie jest aktywny"]);

            return new ErrorResultDTO<UsersDTO>(["Błędny e-mail lub hasło"]);
        }

        public Task LogoutAsync() => _signInManager.SignOutAsync();

        public async Task<ResultDTO<UsersDTO>> Me(ClaimsPrincipal User)
        {
            var user = await _userManager.GetUserAsync(User);
            var userProfile = await _usersRepository.GetByUserIdAsync(user?.Id ?? string.Empty, CancellationToken.None);
            if (user is null || userProfile is null)
            {
                return new ErrorResultDTO<UsersDTO>(["Nie znaleziono profilu użytkownika"]);
            }
            return new SuccessResultDTO<UsersDTO>
            {
                Succeeded = true,
                Data = GetUserDTO(userProfile, user, _usersRepository.GetUserConfiguration(user.Id))
            };
        }

        public async Task<(int userId, List<string> rolesIds)> LoggedUserRoles(ClaimsPrincipal User)
        {
            var user = await _userManager.GetUserAsync(User);
            return await _usersRepository.GetLoggedUserRolesAsync(user?.Id ?? string.Empty, CancellationToken.None);
        }

        public bool IsLoggedIn()
         => _http.HttpContext?.User?.Identity?.IsAuthenticated == true;

        public async Task<IReadOnlyList<UsersDTO>> GetAllUsersAsync(CancellationToken ct = default)
        {
            var users = await _userManager.Users.ToListAsync(ct);
            if (users.Count == 0) return [];

            var ids = users.Select(u => u.Id).ToList();

            var profiles = await _usersRepository.GetUsersProfilesAsync(ct);
            var profileByUserId = profiles.ToDictionary(p => p.UserId ?? "", p => p);

            return [.. users.Select(u =>
            {
                profileByUserId.TryGetValue(u.Id, out var p);
                return (user: u, profile: p);
            })
            .Where(x => x.profile != null)
            .Select(x => GetUserDTO(x.profile!, x.user))];
        }

        public async Task<UsersDTO> GetUserByIdAsync(int id, CancellationToken ct = default)
        {
            var userProfile = await _usersRepository.GetReadOnlyAsync(id, ct) ?? throw new KeyNotFoundException($"Nie znaleziono profilu użytkownika");
            var appUser = await _userManager.FindByIdAsync(userProfile.UserId!) ?? throw new KeyNotFoundException($"Nie znaleziono użytkownika {userProfile.DisplayName}");
            return GetUserDTO(userProfile, appUser, _usersRepository.GetUserConfiguration(userProfile.UserId!));
        }

        public async Task<ApplicationUser?> GetUserByEmail(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<ResultDTO<object>> UpdateOthersPassword(int userId, string newPassword)
        {
            var userDTO = await GetUserByIdAsync(userId);
            if (userDTO == null) return new ErrorResultDTO<object>(["Nie znaleziono profilu użytkownika"]);
            var user = await _userManager.FindByIdAsync(userDTO.UserId);
            if (user == null) return new ErrorResultDTO<object>(["Nie znaleziono profilu użytkownika"]);

            IdentityResult result;
            if (await _userManager.HasPasswordAsync(user))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            }
            else
                result = await _userManager.AddPasswordAsync(user, newPassword);

            if (result.Succeeded)
                return new SuccessResultDTO<object>(userDTO.UserId, ["Zaktualizowano hasło!"]);

            return new ErrorResultDTO<object>(MapPasswordChangeErrorCode([.. result.Errors]));
        }

        public async Task<ResultDTO<object>> UpdateOwnPassword(int userId, string oldPassword, string newPassword)
        {
            var userDTO = await GetUserByIdAsync(userId);
            if (userDTO == null) return new ErrorResultDTO<object>(["Nie znaleziono profilu użytkownika"]);
            var user = await _userManager.FindByIdAsync(userDTO.UserId);
            if (user == null) return new ErrorResultDTO<object>(["Nie znaleziono profilu użytkownika"]);

            IdentityResult result;
            if (await _userManager.HasPasswordAsync(user))
            {
                if (string.IsNullOrEmpty(oldPassword))
                    return new ErrorResultDTO<object>(["Podaj stare hasło"]);

                result = await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            }
            else
                result = await _userManager.AddPasswordAsync(user, newPassword);

            if (result.Succeeded)
                return new SuccessResultDTO<object>(userDTO.UserId, ["Zaktualizowano hasło!"]);

            return new ErrorResultDTO<object>(MapPasswordChangeErrorCode([.. result.Errors]));
        }

        private static string[] MapPasswordChangeErrorCode(List<IdentityError> errors)
        {
            var errorsPL = new List<string>();
            foreach(var error in errors)
            {
                var errorCodePL = IdentityErrorPL.Map.FirstOrDefault(x => x.Key == error.Code);
                if (!string.IsNullOrWhiteSpace(errorCodePL.Value) && !errorsPL.Contains(errorCodePL.Value))
                    errorsPL.Add(errorCodePL.Value);
            }

            return [.. errorsPL];
        }

        public async Task<bool> AdminResetPassword(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return false;

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result.Succeeded;
        }

        public async Task<IReadOnlyList<UsersDTO>> GetUsers(CancellationToken ct = default)
        {
            var usersProfiles = await _usersRepository.ListAllAsync(ct);
            var appUsers = await _userManager.Users.ToListAsync(ct);
            return [.. (from u in usersProfiles
                    join appU in appUsers
                    on u.UserId equals appU.Id
                    select GetUserDTO(u, appU)
                )];
        }

        private static UsersDTO GetUserDTO(UsersProfiles userProfile, ApplicationUser appUser, UserConfiguration? userConfiguration = null)
        {
            var alternativeEmails = string.IsNullOrWhiteSpace(userProfile.AlternativeEmails) ? [] : 
                    userProfile.AlternativeEmails.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim()).ToArray();
            return new UsersDTO(
                    userProfile,
                    appUser.Email,
                    alternativeEmails,
                    appUser.PhoneNumber, 
                    userConfiguration
            );
        }

        public async Task SendAccountCreatedEmail(ApplicationUser user, UsersProfiles userProfile, CancellationToken ct = default)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetUrl = $"{_appUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={tokenEncoded}";

            var (html, text) = _emailSender.BuildPasswordEmail(userProfile.DisplayName ?? user.Email!, resetUrl, isInvite: true);

            await _emailSender.SendAsync(
                [user.Email!], "Twoje konto CrmApp", html, 
                ct, text);
        }

        public async Task<ResultDTO<string>> ForgotPassword(ForgotPasswordDTO req, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user is null) return new ErrorResultDTO<string>(["Konto o podanym adresie e-mail nie istnieje w systemie"]);

            var userProfile = await _usersRepository.GetByUserIdAsync(user.Id, ct);
            if (userProfile!.IsDeleted) return new ErrorResultDTO<string>(["Konto zostało dezaktywowane"]);

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var tokenEncoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var resetUrl = $"{_appUrl}/reset-password?email={Uri.EscapeDataString(req.Email)}&token={tokenEncoded}";

            var (html, text) = _emailSender.BuildPasswordEmail(userProfile.DisplayName ?? req.Email, resetUrl, isInvite: false);

            await _emailSender.SendAsync([req.Email], "CrmApp: Zresetuj swoje hasło", html, ct, text);
            return new SuccessResultDTO<string>("", ["E-mail z linkiem do zresetowania hasła został wysłany!"]);
        }

        public async Task<ResultDTO<string>> ResetPasswordWithToken(ResetPasswordWthTokenDTO req, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(req.Email);
            if (user is null) return new ErrorResultDTO<string>(["Konto o podanym adresie e-mail nie istnieje w systemie"]);

            var userProfile = await _usersRepository.GetByUserIdAsync(user.Id, ct);
            if (userProfile!.IsDeleted) return new ErrorResultDTO<string>(["Konto zostało dezaktywowane"]);

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(req.Token));
            var result = await _userManager.ResetPasswordAsync(user, token, req.NewPassword);
            if (!result.Succeeded) return new ErrorResultDTO<string>(MapPasswordChangeErrorCode([.. result.Errors]));

            if (result.Succeeded) await _userManager.UpdateSecurityStampAsync(user);

            return new SuccessResultDTO<string>("", ["Zresetowano hasło. Zaloguj się do swojego konta."]);
        }

        public async Task<ResultDTO<object>> ActivateAccount(int id, CancellationToken ct = default)
        {
            var userProfile = await _usersRepository.GetAsync(id, ct);
            if (userProfile == null)
                return new ErrorResultDTO<object>(["Nie znaleziono profilu użytkownika w bazie"]);
            
            if (string.IsNullOrWhiteSpace(userProfile.UserId))
                return new ErrorResultDTO<object>(["Użytkownik nie posiada konta w aplikacji"]);

            var appUser = await _userManager.FindByIdAsync(userProfile.UserId) ?? throw new KeyNotFoundException("Użytkownik nie posiada konta w aplikacji");
            await _userManager.SetLockoutEnabledAsync(appUser, false);
            await _usersRepository.Activate(userProfile);
            await _usersRepository.SaveChangesAsync(ct);

            return new SuccessResultDTO<object>(id.ToString(), []);
        }
    }
}
