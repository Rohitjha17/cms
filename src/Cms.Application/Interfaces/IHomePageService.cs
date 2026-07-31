using Cms.Application.DTOs.HomePage;

namespace Cms.Application.Interfaces;

public interface IHomePageService
{
    Task<HomePageResponseDto> GetHomePageAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HomePageSectionDto>> GetSectionsAsync(bool includeInactive = true, CancellationToken cancellationToken = default);
    Task<HomePageSectionDto> GetSectionAsync(string sectionKey, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<HomePageSectionDto> CreateSectionAsync(CreateHomePageSectionDto dto, CancellationToken cancellationToken = default);
    Task<HomePageSectionDto> UpdateSectionAsync(string sectionKey, UpdateHomePageSectionDto dto, CancellationToken cancellationToken = default);
    Task SetStatusAsync(string sectionKey, bool isActive, CancellationToken cancellationToken = default);
    Task ReorderAsync(ReorderHomePageSectionsDto dto, CancellationToken cancellationToken = default);
    Task DeleteSectionAsync(string sectionKey, bool hardDelete = false, CancellationToken cancellationToken = default);
    Task EnsureDefaultSectionsAsync(CancellationToken cancellationToken = default);
}
