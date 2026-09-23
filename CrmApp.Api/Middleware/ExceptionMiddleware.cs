using Microsoft.AspNetCore.Mvc.Infrastructure;
using CrmApp.Application.Common.Exceptions;

namespace CrmApp.Api.Middleware
{
    public class ExceptionMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionMiddleware> _log;
        private readonly ProblemDetailsFactory _pdf;

        public ExceptionMiddleware(ILogger<ExceptionMiddleware> log, ProblemDetailsFactory pdf)
            => (_log, _pdf) = (log, pdf);

        public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
        {
            try
            {
                await next(ctx);
            }
            catch (OperationCanceledException) when (ctx.RequestAborted.IsCancellationRequested)
            {
                ctx.Response.StatusCode = StatusCodes.Status499ClientClosedRequest; // lub 400
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Unhandled exception");

                var (status, title) = ex switch
                {
                    NotFoundException => (404, "Nie znaleziono"),
                    ValidationException => (400, "Błąd walidacji"),
                    ConflictException => (409, "Konflikt"),
                    ConcurrencyException => (409, "Konflikt współbieżności"),
                    ForbiddenException => (403, "Brak uprawnień"),
                    _ => (500, "Błąd serwera")
                };

                var pd = _pdf.CreateProblemDetails(ctx, statusCode: status, title: title, detail: ex.Message);
                ctx.Response.ContentType = "application/problem+json";
                ctx.Response.StatusCode = status;
                await ctx.Response.WriteAsJsonAsync(pd);
            }
        }
    }
}
