using Cms.Infrastructure.Storage;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// A school's bucket is not public, and asking the customer to make it public is asking them to
/// change something in their own AWS account that they may refuse, defer, or get wrong. When
/// they do, every photograph on the site turns into a broken icon and nothing says why — the
/// upload succeeded, so the console looks fine.
///
/// So the stored address must not depend on the bucket being readable by the world.
/// </summary>
public sealed class PrivateBucketMediaTests
{
    private const string Key = "11111111-1111-1111-1111-111111111111/2222/media/photo.jpg";

    [Fact]
    public void APrivateBucket_IsServedThroughTheApplication()
    {
        var url = S3StorageService.PublicUrl(
            new AwsOptions { BucketName = "school-media", Region = "ap-south-1" }, Key);

        Assert.Equal("/uploads/" + Key, url);
        Assert.DoesNotContain("amazonaws.com", url, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ABucketDeclaredPublic_IsLinkedDirectly()
    {
        var url = S3StorageService.PublicUrl(
            new AwsOptions { BucketName = "school-media", Region = "ap-south-1", PublicBucket = true },
            Key);

        Assert.Equal($"https://school-media.s3.ap-south-1.amazonaws.com/{Key}", url);
    }

    [Fact]
    public void ACdnInFront_WinsOverBothOfThem()
    {
        var url = S3StorageService.PublicUrl(
            new AwsOptions
            {
                BucketName = "school-media",
                Region = "ap-south-1",
                PublicBucket = true,
                PublicBaseUrl = "https://cdn.school.edu.in/"
            },
            Key);

        Assert.Equal($"https://cdn.school.edu.in/{Key}", url);
    }

    /// <summary>
    /// A customer who grants write access to one folder rather than the whole bucket has chosen
    /// where this application must write. Ignoring that setting is what made every upload fail
    /// with "not authorized to perform: s3:PutObject" on a key nobody had agreed to.
    /// </summary>
    [Theory]
    [InlineData("WebSiteData", "WebSiteData/tenant/site/media/a.jpg")]
    [InlineData("/WebSiteData/", "WebSiteData/tenant/site/media/a.jpg")]
    [InlineData("  ", "tenant/site/media/a.jpg")]
    [InlineData(null, "tenant/site/media/a.jpg")]
    public void TheConfiguredFolder_PrefixesEveryKey(string? folder, string expected)
        => Assert.Equal(
            expected,
            S3StorageService.Key(new AwsOptions { FolderPath = folder }, "tenant/site/media/a.jpg"));

    /// <summary>
    /// The key comes from the URL, so it decides which object is read out of the bucket. A path
    /// that climbs out of the prefix this application writes under must never become one.
    /// </summary>
    [Theory]
    [InlineData("/tenant/site/media/photo.jpg", "tenant/site/media/photo.jpg")]
    [InlineData("tenant/site/media/photo.jpg", "tenant/site/media/photo.jpg")]
    [InlineData("/../secrets/key.txt", null)]
    [InlineData("/tenant/../../etc/passwd", null)]
    [InlineData("/tenant//site/photo.jpg", null)]
    [InlineData("/tenant\\site\\photo.jpg", null)]
    [InlineData("/", null)]
    [InlineData("", null)]
    public void OnlyAPlainKeyIsAccepted(string remainder, string? expected)
        => Assert.Equal(expected, S3MediaProxyMiddleware.Key(remainder));
}
