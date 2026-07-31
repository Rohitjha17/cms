using Cms.Application.Interfaces;

namespace Cms.Infrastructure.Tenancy;

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }
    public string? TenantCode { get; private set; }
    public string? TenantName { get; private set; }
    public bool IsResolved => TenantId.HasValue;

    public void Set(Guid tenantId, string code, string name)
    {
        TenantId = tenantId;
        TenantCode = code;
        TenantName = name;
    }
}
