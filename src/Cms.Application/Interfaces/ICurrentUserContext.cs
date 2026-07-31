namespace Cms.Application.Interfaces;

public interface ICurrentUserContext
{
    string? UserId { get; }
    string? DisplayName { get; }
}
