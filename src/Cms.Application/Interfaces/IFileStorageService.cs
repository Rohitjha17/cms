namespace Cms.Application.Interfaces;

public interface IFileStorageService
{
    Task<StoredFileResult> UploadAsync(Stream content, string fileName, string contentType, string folder, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default);
}

public sealed record StoredFileResult(string Url, string StorageKey, string FileName);
