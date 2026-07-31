using Cms.Application.Interfaces;
using Cms.Domain.Entities;
using Cms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cms.Infrastructure.Repositories;

public class MediaRepository : IMediaRepository
{
    private readonly ApplicationDbContext _db;

    public MediaRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(MediaFile media, CancellationToken cancellationToken = default)
    {
        await _db.MediaFiles.AddAsync(media, cancellationToken);
    }

    public async Task<IReadOnlyList<MediaFile>> GetAllAsync(
        Guid tenantId,
        Guid siteId,
        CancellationToken cancellationToken = default) =>
        await _db.MediaFiles
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.SiteId == siteId)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);

    public Task<MediaFile?> GetByIdAsync(
        Guid tenantId,
        Guid siteId,
        Guid id,
        CancellationToken cancellationToken = default) =>
        _db.MediaFiles.FirstOrDefaultAsync(
            x => x.Id == id && x.TenantId == tenantId && x.SiteId == siteId,
            cancellationToken);

    public void Delete(MediaFile media) => _db.MediaFiles.Remove(media);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
