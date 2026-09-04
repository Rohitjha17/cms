using Cms.Application.DTOs.Websites;

namespace Cms.Application.Interfaces;

public interface IWebsiteService
{
    Task<IReadOnlyList<PageTemplateDto>> GetPageTemplatesAsync(CancellationToken cancellationToken);
    Task<PageTemplateDto> SavePageTemplateAsync(Guid? id, SavePageTemplateDto dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<WebsiteSummaryDto>> GetWebsitesAsync(CancellationToken cancellationToken);
    Task<WebsiteSummaryDto> ProvisionAsync(ProvisionWebsiteDto dto, CancellationToken cancellationToken);

    Task<IReadOnlyList<SiteTemplateSummaryDto>> GetSiteTemplatesAsync(CancellationToken cancellationToken);
    Task<WebsiteSummaryDto> ProvisionFromTemplateAsync(ProvisionFromTemplateDto dto, CancellationToken cancellationToken);

    Task<IReadOnlyList<SiteDomainDto>> GetDomainsAsync(CancellationToken cancellationToken);
    Task<SiteDomainDto> SaveDomainAsync(Guid? id, SaveSiteDomainDto dto, CancellationToken cancellationToken);
    Task DeleteDomainAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<PublicPageDto>> AssignTemplatesAsync(AssignTemplatesDto dto, CancellationToken cancellationToken);
    /// <param name="removedPageSlug">
    /// The slug of a page being deleted, when one is. Its link cannot be identified afterwards:
    /// once the page is gone it looks exactly like a link somebody added by hand.
    /// </param>
    Task SyncHeaderMenuAsync(CancellationToken cancellationToken, string? removedPageSlug = null);
    Task<SiteBrandingDto> GetBrandingAsync(CancellationToken cancellationToken);
    Task<SiteBrandingDto> SaveBrandingAsync(SiteBrandingDto dto, CancellationToken cancellationToken);
    Task<PublicWebsiteDto> GetPublicWebsiteAsync(CancellationToken cancellationToken);
    Task<PublicPageDto> GetPublicPageAsync(string slug, CancellationToken cancellationToken);
    Task<IReadOnlyList<ContactSubmissionDto>> GetContactSubmissionsAsync(CancellationToken cancellationToken);

    /// <summary>Enquiries nobody has opened yet, for the console's notification badge.</summary>
    Task<int> GetUnreadContactCountAsync(CancellationToken cancellationToken);
    Task<ContactSubmissionDto> SubmitContactAsync(SubmitContactDto dto, CancellationToken cancellationToken);
    Task MarkContactReadAsync(Guid id, bool isRead, CancellationToken cancellationToken);
}
