using Cms.Application.DTOs.SchoolContent;

namespace Cms.Application.Interfaces;

/// <summary>
/// Typed access to the school content an institution maintains day to day: faculty and
/// staff, news and notices, events, and the site's operational settings.
/// </summary>
public interface ISchoolContentService
{
    Task<IReadOnlyList<FacultyMemberDto>> GetFacultyAsync(bool includeUnpublished, CancellationToken cancellationToken);
    Task<FacultyMemberDto> GetFacultyMemberAsync(Guid id, CancellationToken cancellationToken);
    Task<FacultyMemberDto> SaveFacultyMemberAsync(Guid? id, SaveFacultyMemberDto dto, CancellationToken cancellationToken);
    Task DeleteFacultyMemberAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<NewsArticleDto>> GetNewsAsync(bool includeUnpublished, CancellationToken cancellationToken);
    Task<NewsArticleDto> GetNewsArticleAsync(Guid id, CancellationToken cancellationToken);
    Task<NewsArticleDto?> GetNewsArticleByKeyAsync(string key, CancellationToken cancellationToken);
    Task<NewsArticleDto> SaveNewsArticleAsync(Guid? id, SaveNewsArticleDto dto, CancellationToken cancellationToken);
    Task DeleteNewsArticleAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<SchoolEventDto>> GetEventsAsync(bool includeUnpublished, CancellationToken cancellationToken);
    Task<SchoolEventDto> GetEventAsync(Guid id, CancellationToken cancellationToken);
    Task<SchoolEventDto?> GetEventByKeyAsync(string key, CancellationToken cancellationToken);
    Task<SchoolEventDto> SaveEventAsync(Guid? id, SaveSchoolEventDto dto, CancellationToken cancellationToken);
    Task DeleteEventAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<DepartmentDto>> GetDepartmentsAsync(bool includeUnpublished, CancellationToken cancellationToken);
    Task<DepartmentDto> GetDepartmentAsync(Guid id, CancellationToken cancellationToken);
    Task<DepartmentDto> SaveDepartmentAsync(Guid? id, SaveDepartmentDto dto, CancellationToken cancellationToken);
    Task DeleteDepartmentAsync(Guid id, CancellationToken cancellationToken);

    Task<SiteSettingsDto> GetSettingsAsync(CancellationToken cancellationToken);
    Task<SiteSettingsDto> SaveSettingsAsync(SiteSettingsDto dto, CancellationToken cancellationToken);
}
