using System.Text.Json;
using Cms.Shared.Exceptions;
using Cms.Shared.Responses;
using FluentValidation;

namespace Cms.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await WriteErrorAsync(context, ex);
        }
    }

    private async Task WriteErrorAsync(HttpContext context, Exception exception)
    {
        var (status, message, errors) = exception switch
        {
            ValidationAppException vae => (vae.StatusCode, vae.Message, vae.Errors),
            ValidationException fve => (400, "Validation failed.", fve.Errors.Select(e => e.ErrorMessage).ToList()),
            AppException ae => (ae.StatusCode, ae.Message, (IEnumerable<string>?)null),
            _ => (500, "An unexpected error occurred.", (IEnumerable<string>?)null)
        };

        if (status >= 500)
        {
            _logger.LogError(exception, "Unhandled exception");
        }
        else
        {
            _logger.LogWarning(exception, "Handled application exception");
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        var payload = ApiResponse.Fail(message, status, errors);
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}
