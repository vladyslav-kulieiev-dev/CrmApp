using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using CrmApp.Application.Abstractions;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Application.Services;
using CrmApp.Domain.DTO;
using CrmApp.Domain.Identity;
using CrmApp.Domain.Tools;
using CrmApp.Infrastructure.Identity;
using CrmApp.Infrastructure.Identity.Authorization;
using CrmApp.Infrastructure.Persistence;
using CrmApp.Infrastructure.Persistence.Repositories;
using CrmApp.Infrastructure.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CrmApp.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, IConfiguration cfg)
        {
            var conn = cfg.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(conn)
                //.EnableDetailedErrors() // dev only
                //.EnableSensitiveDataLogging() // dev only
            );
            services.AddDbContext<AppDbContext>(o => o.UseSqlServer(conn)
                //.EnableDetailedErrors() // dev only
                //.EnableSensitiveDataLogging() // dev only
            );
            services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>();
            services.AddScoped<IAppDbContext>(sp =>
                sp.GetRequiredService<AppDbContext>());

            services.AddHttpContextAccessor();
            services.AddScoped<IZohoDeskService, ZohoDeskService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<ICurrentUser, CurrentUser>();
            services.AddScoped<IUsersRepository, UsersRepository>();
            services.AddScoped<IContractorsRepository, ContractorsRepository>();
            services.AddScoped<IContractorContractsRepository, ContractorContractsRepository>();
            services.AddScoped<IContractorContactsRepository, ContractorContactsRepository>();
            services.AddScoped<IContractorLicensesRepository, ContractorLicensesRepository>();
            services.AddScoped<ISettingsRepository, SettingsRepository>();
            services.AddScoped<IProjectsRepository, ProjectsRepository>();
            services.AddScoped<IDictionariesRepository, DictionariesRepository>();
            services.AddScoped<ICatalogItemsRepository, CatalogItemsRepository>();
            services.AddScoped<ITablesAdditionalFieldsRepository, TablesAdditionalFieldsRepository>();
            services.AddScoped<ITablesAdditionalFieldsValuesRepository, TablesAdditionalFieldsValuesRepository>();
            services.AddScoped<ITablesAdditionalFieldsPermissionsRepository, TablesAdditionalFieldsPermissionsRepository>();
            services.AddScoped<ITasksRepository, TasksRepository>();
            services.AddScoped<IImportedTasksRepository, ImportedTasksRepository>();
            services.AddScoped<ITasksCommentsRepository, TasksCommentsRepository>();
            services.AddScoped<IAdditionalFieldsService, AdditionalFieldsService>();
            services.AddScoped<IDictionariesService, DictionariesService>();
            services.AddScoped<ICatalogItemsService, CatalogItemsService>();
            services.AddScoped<IContractorsService, ContractorsService>();
            services.AddScoped<IContractorsContractsService, ContractorsContractsService>();
            services.AddScoped<IContractorContactsService, ContractorContactsService>();
            services.AddScoped<IContractorLicensesService, ContractorLicensesService>();
            services.AddScoped<IProjectsService, ProjectsService>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IEmailSender, SmtpEmailSender>();
            services.AddScoped<ITasksService, TasksService>();
            services.AddScoped<UsersManager>();
            services.AddScoped<IdentityService>();

            return services;
        }

        public static IServiceCollection AddAuthentication(this IServiceCollection services)
        {
            services
                .AddIdentityCore<ApplicationUser>(o =>
                {
                    o.User.RequireUniqueEmail = true;
                    o.Password.RequiredLength = 6;
                    o.Password.RequireDigit = false;
                    o.Password.RequireUppercase = false;
                    o.Password.RequireNonAlphanumeric = false;
                })
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            services.AddAuthentication(IdentityConstants.ApplicationScheme)
                .AddCookie(IdentityConstants.ApplicationScheme, o =>
                {
                    o.Cookie.Name = "app.auth.v2";
                    o.Cookie.HttpOnly = true;
                    o.Cookie.SameSite = SameSiteMode.Lax;
                    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    o.ExpireTimeSpan = TimeSpan.FromDays(14);
                    o.SlidingExpiration = true;

                    o.Events = new CookieAuthenticationEvents
                    {
                        OnSigningIn = ctx =>
                        {
                            if (ctx.Principal?.Identity is ClaimsIdentity id)
                            {
                                var count = id.Claims.Count();
                                var size = id.Claims.Sum(c => (c.Type?.Length ?? 0) + (c.Value?.Length ?? 0));

                                var roleClaimType = id.RoleClaimType;

                                var toRemove = id.Claims
                                    .Where(c => c.Type == AppClaimTypes.Permission || c.Type == roleClaimType)
                                    .ToList();

                                foreach (var c in toRemove)
                                    id.RemoveClaim(c);

                                var totalClaimsAfter = id.Claims.Count();

                            }

                            return Task.CompletedTask;
                        },
                        OnRedirectToLogin = ctx =>
                        {
                            if (ctx.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase) ||
                                ctx.Request.Path.StartsWithSegments("/api"))
                            {
                                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                return Task.CompletedTask;
                            }
                            ctx.Response.Redirect(ctx.RedirectUri);
                            return Task.CompletedTask;
                        },
                        OnRedirectToAccessDenied = ctx =>
                        {
                            if (ctx.Request.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase) ||
                                ctx.Request.Path.StartsWithSegments("/api"))
                            {
                                ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                                return Task.CompletedTask;
                            }
                            ctx.Response.Redirect(ctx.RedirectUri);
                            return Task.CompletedTask;
                        }
                    };
                });

            //// FOR COOLIFY DEPLOY
            //services.AddAntiforgery(opt =>
            //{
            //    opt.HeaderName = "X-XSRF-TOKEN";
            //    opt.Cookie.Name = "XSRF-TOKEN";
            //    opt.Cookie.SameSite = SameSiteMode.Strict;
            //    opt.Cookie.HttpOnly = false; // so Angular can read & echo it
            //});

            return services;
        }
    }

}
