using Cms.Application.DTOs.Tenancy;
using Cms.Application.Interfaces;
using Cms.Domain.Entities;
using Cms.Domain.Enums;
using Cms.Shared.Exceptions;
using FluentValidation;

namespace Cms.Application.Services;

public sealed class TenantManagementService : ITenantManagementService
{
    private readonly ITenantManagementRepository _repository;
    private readonly IValidator<SaveTenantDto> _validator;
    private readonly ICurrentUserContext _currentUser;

    public TenantManagementService(
        ITenantManagementRepository repository,
        IValidator<SaveTenantDto> validator,
        ICurrentUserContext currentUser)
    {
        _repository = repository;
        _validator = validator;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TenantManagementDto>> GetAllAsync(CancellationToken cancellationToken) =>
        (await _repository.GetAllAsync(cancellationToken)).Select(ToDto).ToList();

    public async Task<TenantManagementDto> GetAsync(Guid id, CancellationToken cancellationToken) =>
        ToDto(await _repository.GetAsync(id, cancellationToken)
            ?? throw new NotFoundException("Tenant was not found."));

    public async Task<TenantManagementDto> SaveAsync(
        Guid? id, SaveTenantDto dto, CancellationToken cancellationToken)
    {
        dto.Code = dto.Code.Trim().ToLowerInvariant();
        dto.Domains.ForEach(x => x.DomainName = x.DomainName.Trim().ToLowerInvariant());
        dto.Sites.ForEach(x => x.SiteKey = x.SiteKey.Trim().ToLowerInvariant());
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var duplicateCode = await _repository.GetByCodeAsync(dto.Code, cancellationToken);
        if (duplicateCode is not null && duplicateCode.Id != id)
        {
            throw new ValidationAppException($"Tenant code '{dto.Code}' is already in use.");
        }
        foreach (var domain in dto.Domains)
        {
            var duplicateDomain = await _repository.GetDomainAsync(domain.DomainName, cancellationToken);
            if (duplicateDomain is not null && duplicateDomain.TenantId != id)
            {
                throw new ValidationAppException($"Domain '{domain.DomainName}' is already assigned.");
            }
        }

        Tenant tenant;
        if (id.HasValue)
        {
            tenant = await _repository.GetAsync(id.Value, cancellationToken)
                ?? throw new NotFoundException("Tenant was not found.");
            tenant.UpdatedDate = DateTime.UtcNow;
            tenant.UpdatedBy = Actor;
        }
        else
        {
            tenant = new Tenant { CreatedDate = DateTime.UtcNow, CreatedBy = Actor };
            await _repository.AddAsync(tenant, cancellationToken);
        }

        tenant.Name = dto.Name.Trim();
        tenant.Code = dto.Code;
        tenant.LogoUrl = dto.LogoUrl?.Trim();
        tenant.IsActive = dto.IsActive;

        foreach (var existing in tenant.Sites)
        {
            existing.IsActive = false;
            existing.IsDefault = false;
        }
        foreach (var input in dto.Sites)
        {
            var site = tenant.Sites.FirstOrDefault(x => x.SiteKey == input.SiteKey);
            if (site is null)
            {
                site = new Site { SiteKey = input.SiteKey, CreatedDate = DateTime.UtcNow, CreatedBy = Actor };
                tenant.Sites.Add(site);
            }
            site.Name = input.Name.Trim();
            site.WebsiteType = Enum.Parse<WebsiteType>(input.WebsiteType, true);
            site.HomeVariant = Enum.TryParse<HomeVariant>(input.HomeVariant, true, out var variant)
                ? variant
                : HomeVariant.Classic;
            site.IsDefault = input.IsDefault;
            site.IsActive = input.IsActive;
        }

        // Only additions are accepted here, as a convenience when creating a tenant.
        // Editing, unbinding and removal belong to the Domains screen, which enforces
        // global host uniqueness and refuses to take the last live host off a website.
        foreach (var input in dto.Domains.Where(x => !string.IsNullOrWhiteSpace(x.DomainName)))
        {
            if (tenant.Domains.Any(x => x.DomainName == input.DomainName))
            {
                continue;
            }

            var boundSite = string.IsNullOrWhiteSpace(input.SiteKey)
                ? null
                : tenant.Sites.FirstOrDefault(s =>
                    string.Equals(s.SiteKey, input.SiteKey.Trim(), StringComparison.OrdinalIgnoreCase));

            tenant.Domains.Add(new TenantDomain
            {
                DomainName = input.DomainName,
                SiteId = boundSite?.Id,
                IsPrimary = input.IsPrimary,
                IsActive = input.IsActive,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = Actor
            });
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(tenant);
    }

    private string Actor => _currentUser.UserId ?? "system";

    private static TenantManagementDto ToDto(Tenant tenant) => new()
    {
        Id = tenant.Id, Name = tenant.Name, Code = tenant.Code,
        LogoUrl = tenant.LogoUrl, IsActive = tenant.IsActive,
        Domains = tenant.Domains.OrderByDescending(x => x.IsPrimary).Select(x =>
        {
            var siteKey = x.SiteId.HasValue
                ? tenant.Sites.FirstOrDefault(s => s.Id == x.SiteId)?.SiteKey
                : null;
            return new TenantDomainInputDto
            {
                DomainName = x.DomainName,
                SiteKey = siteKey,
                IsPrimary = x.IsPrimary,
                IsActive = x.IsActive
            };
        }).ToList(),
        Sites = tenant.Sites.OrderByDescending(x => x.IsDefault).Select(x => new TenantSiteInputDto
        {
            Name = x.Name, SiteKey = x.SiteKey, WebsiteType = x.WebsiteType.ToString(),
            HomeVariant = x.HomeVariant.ToString(),
            IsDefault = x.IsDefault, IsActive = x.IsActive
        }).ToList()
    };
}
