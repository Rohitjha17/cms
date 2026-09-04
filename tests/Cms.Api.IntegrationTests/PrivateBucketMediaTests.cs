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
    private const string Path = "11111111-1111-1111-1111-111111111111/2222/media/photo.jpg";
    private const string Key = "WebSiteData/" + Path;

    [Fact]
    public void APrivateBucket_IsServedThroughTheApplication()
    {
        var url = S3StorageService.PublicUrl(
            new AwsOptions { BucketName = "school-media", Region = "ap-south-1" }, Key, Path);

        // The bucket folder is deliberately absent: this address is stored in the database
        // beside the file and must survive a change to FolderPath, or a move between local
        // storage and S3. The folder is applied when the object is fetched, not here.
        Assert.Equal("/uploads/" + Path, url);
        Assert.DoesNotContain("WebSiteData", url, StringComparison.Ordinal);
        Assert.DoesNotContain("amazonaws.com", url, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ABucketDeclaredPublic_IsLinkedDirectly()
    {
        var url = S3StorageService.PublicUrl(
            new AwsOptions { BucketName = "school-media", Region = "ap-south-1", PublicBucket = true },
            Key, Path);

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
            Key, Path);

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

    /// <summary>
    /// The folder is applied on the way in. Both shapes of address have to find the same
    /// object: the one stored now, which does not name the folder, and every one stored before
    /// this change, which does. Getting that wrong turns the whole media library into broken
    /// icons — for the second shape, permanently, because it is already in the database.
    /// </summary>
    [Theory]
    [InlineData("WebSiteData", "/tenant/site/media/a.jpg", "WebSiteData/tenant/site/media/a.jpg")]
    [InlineData("WebSiteData", "/WebSiteData/tenant/site/media/a.jpg", "WebSiteData/tenant/site/media/a.jpg")]
    [InlineData("/WebSiteData/", "/tenant/site/media/a.jpg", "WebSiteData/tenant/site/media/a.jpg")]
    [InlineData(null, "/tenant/site/media/a.jpg", "tenant/site/media/a.jpg")]
    [InlineData("  ", "/tenant/site/media/a.jpg", "tenant/site/media/a.jpg")]
    public void TheBucketFolderIsAppliedWhenTheObjectIsFetched(
        string? folder, string remainder, string expected)
        => Assert.Equal(expected, S3MediaProxyMiddleware.Key(remainder, folder));

    /// <summary>A path that climbs out is still refused, folder or no folder.</summary>
    [Theory]
    [InlineData("WebSiteData", "/../secrets/key.txt")]
    [InlineData("WebSiteData", "/tenant/../../etc/passwd")]
    [InlineData("WebSiteData", "/tenant\\site\\photo.jpg")]
    public void TheFolderNeverRescuesAPathThatClimbsOut(string folder, string remainder)
        => Assert.Null(S3MediaProxyMiddleware.Key(remainder, folder));
}
