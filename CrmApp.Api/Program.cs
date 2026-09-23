
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using CrmApp.Api.Middleware;
using CrmApp.Application.Abstractions;
using CrmApp.Application.Abstractions.Persistence;
using CrmApp.Application.Abstractions.Services;
using CrmApp.Application.Services;
using CrmApp.Domain.Configuration;
using CrmApp.Domain.DTO;
using CrmApp.Domain.DTO.Options;
using CrmApp.Domain.Identity;
using CrmApp.Infrastructure;
using CrmApp.Infrastructure.Identity;
using CrmApp.Infrastructure.Identity.Authorization;
using CrmApp.Infrastructure.Persistence;
using CrmApp.Infrastructure.Persistence.Repositories;
using CrmApp.Infrastructure.Security;
using System;

var builder = WebApplication.CreateBuilder(args);
var conn = builder.Configuration.GetConnectionString("DefaultConnection");

const string FrontendCors = "FrontendCors";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCors, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.Configure<FrontendOptions>(builder.Configuration.GetSection("Frontend"));

DependencyInjection.AddInfrastructure(builder.Services, builder.Configuration);
DependencyInjection.AddAuthentication(builder.Services);

builder.Services.AddAuthorization();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddMemoryCache();

builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddSqlServer()
        .WithGlobalConnectionString(conn)
        .ScanIn(typeof(CrmApp.Migrations._2026._09._20260923_120000_InitialCrmSchema).Assembly).For.All())
    .AddLogging(lb => lb.AddFluentMigratorConsole());


builder.Services.AddHttpContextAccessor();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<AdminUserOptions>(builder.Configuration.GetSection("Identity:Admin"));
builder.Services.AddScoped<IIdentitySeeder, IdentitySeeder>();
builder.Services.AddTransient<ExceptionMiddleware>();
builder.Services.AddHttpClient();


// FOR COOLIFY DEPLOY
//builder.Services.AddDataProtection()
//    .PersistKeysToFileSystem(new DirectoryInfo("/keys"))    // container path
//    .SetApplicationName("example-app");

var app = builder.Build();
app.UseForwardedHeaders(new ForwardedHeadersOptions
                        {
                            ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
                        });

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors(FrontendCors);

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI(opt =>
    {
        opt.RoutePrefix = "swagger"; // UI at /swagger
        opt.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
    });
//}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("/index.html");
//app.MapFallback(context =>
//{
//    var path = context.Request.Path;
//    if (path.StartsWithSegments("/swagger") || path.StartsWithSegments("/api"))
//    {
//        // Let other middleware/handlers deal with it (don�t fallback to SPA)
//        context.Response.StatusCode = StatusCodes.Status404NotFound;
//        return Task.CompletedTask;
//    }

//    context.Response.ContentType = "text/html; charset=utf-8";
//    return context.Response.SendFileAsync(Path.Combine(app.Environment.WebRootPath, "index.html"));
//});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();

    await scope.ServiceProvider.GetRequiredService<IIdentitySeeder>().SeedAsync();
}

app.Run();