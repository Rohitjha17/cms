namespace Cms.Application.Interfaces;

public interface ICurrentUserContext
{
    string? UserId { get; }
    string? DisplayName { get; }

    /// <summary>True when the signed-in account is a platform super administrator.</summary>
    bool IsSuperAdmin { get; }

    bool IsInRole(string role);
}
