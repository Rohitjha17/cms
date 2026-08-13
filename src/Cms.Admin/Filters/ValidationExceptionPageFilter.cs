using Cms.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Cms.Admin.Filters;

/// <summary>
/// A page whose lists must be refetched before the form is redisplayed after a failed save.
/// Without this the page would render with empty tables beside the error message.
/// </summary>
public interface IReloadablePage
{
    Task ReloadAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Turns a failed save into an error on the form instead of a stack trace.
///
/// Services validate with FluentValidation, which throws <see cref="ValidationException"/>,
/// and raise <see cref="ValidationAppException"/> for rule violations. Any handler that did
/// not catch both showed the developer exception page — for example creating a user with a
/// password missing an uppercase letter. Handling it centrally means every screen behaves
/// the same and no new screen can forget.
/// </summary>
public sealed class ValidationExceptionPageFilter : IAsyncPageFilter
{
    private readonly ILogger<ValidationExceptionPageFilter> _logger;

    public ValidationExceptionPageFilter(ILogger<ValidationExceptionPageFilter> logger) =>
        _logger = logger;

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) =>
        Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var executed = await next();

        if (executed.Exception is null || executed.ExceptionHandled)
        {
            return;
        }

        var messages = Describe(executed.Exception);
        if (messages.Count == 0)
        {
            // Not a validation problem — let the error page handle it.
            return;
        }

        foreach (var message in messages)
        {
            context.ModelState.AddModelError(string.Empty, message);
        }

        // Redisplaying a list screen with empty tables would look like data loss, so give the
        // page a chance to refetch what it needs.
        if (context.HandlerInstance is IReloadablePage reloadable)
        {
            try
            {
                await reloadable.ReloadAsync(context.HttpContext.RequestAborted);
            }
            catch (Exception reloadFailure)
            {
                _logger.LogError(reloadFailure, "Reloading the page after a validation failure failed.");
            }
        }

        executed.ExceptionHandled = true;
        executed.Result = new PageResult();
    }

    private static IReadOnlyList<string> Describe(Exception exception) => exception switch
    {
        FluentValidation.ValidationException validation =>
            validation.Errors.Select(x => x.ErrorMessage).Distinct().ToList(),
        ValidationAppException appValidation => appValidation.Errors.ToList(),
        NotFoundException or UnauthorizedAppException or ForbiddenAppException => [],
        AppException app => [app.Message],
        _ => []
    };
}
