namespace Cms.Application.Interfaces;

/// <summary>
/// Transactional mail for account invitations and password resets.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// True when a real transport is configured. Callers use this to decide whether a
    /// reset link must be surfaced in the UI for manual delivery instead.
    /// </summary>
    bool IsConfigured { get; }

    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
