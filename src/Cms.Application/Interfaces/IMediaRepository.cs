using Cms.Domain.Entities;

namespace Cms.Application.Interfaces;

public interface IMediaRepository
{
    Task AddAsync(MediaFile media, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MediaFile>> GetAllAsync(Guid tenantId, Guid siteId, CancellationToken cancellationToken = default);
    Task<MediaFile?> GetByIdAsync(Guid tenantId, Guid siteId, Guid id, CancellationToken cancellationToken = default);
    void Delete(MediaFile media);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
