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
    public string? PublicBaseUrl { get; set; }
}
