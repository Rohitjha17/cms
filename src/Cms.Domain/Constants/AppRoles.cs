namespace Cms.Domain.Constants;

public static class AppRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string TenantAdmin = "TenantAdmin";
    public const string Editor = "Editor";

    public static readonly IReadOnlyList<string> All = [SuperAdmin, TenantAdmin, Editor];
}

public static class AppClaimTypes
{
    public const string TenantId = "tenant_id";
}
