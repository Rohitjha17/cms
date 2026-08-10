using Cms.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Cms.Infrastructure.Email;

/// <summary>
/// Fallback used when no SMTP host is configured. Reports itself as unconfigured so the
/// CMS shows the one-time reset link on screen for the administrator to pass on instead
/// of silently pretending an email was delivered.
/// </summary>
public sealed class NullEmailSender : IEmailSender
{
    private readonly ILogger<NullEmailSender> _logger;

    public NullEmailSender(ILogger<NullEmailSender> logger)
    {
        _logger = logger;
    }

    public bool IsConfigured => false;

    public Task SendAsync(
        string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Email suppressed (no SMTP configured). Would have sent '{Subject}' to {Recipient}.",
            subject, to);
        return Task.CompletedTask;
    }
}
