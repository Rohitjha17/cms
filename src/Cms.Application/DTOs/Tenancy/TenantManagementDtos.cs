namespace Cms.Application.DTOs.Tenancy;

public sealed class TenantManagementDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; }
    public List<TenantDomainInputDto> Domains { get; set; } = [];
    public List<TenantSiteInputDto> Sites { get; set; } = [];
}

public sealed class SaveTenantDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public List<TenantDomainInputDto> Domains { get; set; } = [];
    public List<TenantSiteInputDto> Sites { get; set; } = [];
}

public sealed class TenantDomainInputDto
{
    public string DomainName { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class TenantSiteInputDto
{
    public string Name { get; set; } = string.Empty;
    public string SiteKey { get; set; } = string.Empty;
    public string WebsiteType { get; set; } = "School";
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
}
