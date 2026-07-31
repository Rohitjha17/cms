using Cms.Application.DTOs.Content;

namespace Cms.Application.Interfaces;

public interface ISiteContentService
{
    Task<IReadOnlyList<PageDto>> GetPagesAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<PageDto> GetPageAsync(Guid id, CancellationToken cancellationToken);
    Task<PageDto> GetPageBySlugAsync(string slug, bool includeInactive, CancellationToken cancellationToken);
    Task<PageDto> SavePageAsync(Guid? id, SavePageDto dto, CancellationToken cancellationToken);
    Task DeletePageAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<MenuDto>> GetMenusAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<MenuDto> GetMenuAsync(Guid id, CancellationToken cancellationToken);
    Task<MenuDto> GetMenuByLocationAsync(string location, CancellationToken cancellationToken);
    Task<MenuDto> SaveMenuAsync(Guid? id, SaveMenuDto dto, CancellationToken cancellationToken);
    Task DeleteMenuAsync(Guid id, CancellationToken cancellationToken);

    Task<SeoSettingDto> GetSeoAsync(CancellationToken cancellationToken);
    Task<SeoSettingDto> SaveSeoAsync(SeoSettingDto dto, CancellationToken cancellationToken);

    Task<IReadOnlyList<ContentEntryDto>> GetEntriesAsync(string type, bool includeInactive, CancellationToken cancellationToken);
    Task<ContentEntryDto> GetEntryAsync(Guid id, CancellationToken cancellationToken);
    Task<ContentEntryDto> GetEntryByKeyAsync(string type, string key, bool includeInactive, CancellationToken cancellationToken);
    Task<ContentEntryDto> SaveEntryAsync(Guid? id, SaveContentEntryDto dto, CancellationToken cancellationToken);
    Task DeleteEntryAsync(Guid id, CancellationToken cancellationToken);
}
