namespace Cms.Application.Interfaces;

public interface ITenantContext
{
    Guid? TenantId { get; }
    string? TenantCode { get; }
    string? TenantName { get; }
    bool IsResolved { get; }
    void Set(Guid tenantId, string code, string name);
}
