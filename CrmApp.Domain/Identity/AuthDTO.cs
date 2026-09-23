using CrmApp.Domain.DTO;
using CrmApp.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Domain.Identity
{
    public class RegisterDTO
    {
        public UsersDTO UserDTO { get; set; }
        [MinLength(6)]
        public string Password { get; set; }
    } 

    public record LoginDTO(
        [Required, EmailAddress] string Email,
        [Required, MinLength(6)] string Password,
        bool RememberMe = true,
        bool LockoutOnFailure = true
    );

    public record ResetPasswordDTO(
        [Required] int UserId,
        string OldPassword,
        [Required, MinLength(6)] string NewPassword
    );
    public sealed record ForgotPasswordDTO(string Email);
    public sealed record ResetPasswordWthTokenDTO(string Email, string Token, string NewPassword);
}
