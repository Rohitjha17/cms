namespace Cms.Infrastructure.Storage;

public class StorageOptions
{
    public const string SectionName = "Storage";
    public string Provider { get; set; } = "Local";
    public string LocalRootPath { get; set; } = "wwwroot/uploads";
    public string LocalBaseUrl { get; set; } = "/uploads";
}

public class AwsOptions
{
    public const string SectionName = "Aws";
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// A CDN in front of the bucket. Set it and uploaded files are linked there directly.
    /// </summary>
    public string? PublicBaseUrl { get; set; }

    /// <summary>
    /// Whether the bucket allows anonymous reads. Off by default: the safe assumption is that
    /// it does not, and the application serves the files itself. Turn it on only when the bucket
    /// really is public — it saves a hop, and is wrong in a way that shows as broken images.
    /// </summary>
    public bool PublicBucket { get; set; }

    /// <summary>
    /// A prefix every object is written under, so one bucket can be shared with other systems.
    ///
    /// It also matters for permissions: a customer who grants write access to one folder rather
    /// than the whole bucket has, in effect, chosen where this application must write. Without
    /// this the upload is refused on a path nobody agreed to, and the message names a key that
    /// looks like nothing the customer configured.
    /// </summary>
    public string? FolderPath { get; set; }
}
