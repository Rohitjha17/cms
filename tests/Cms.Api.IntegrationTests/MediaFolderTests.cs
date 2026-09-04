using Cms.Infrastructure.Storage;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// The bucket used to be folders of ids — 11111111-1111-…/22222222-2222-… — so nobody opening
/// it could tell whose content it was, or which of two schools they were about to delete.
///
/// The names are readable now, and still cannot collide: a tenant's code is unique across the
/// platform and a website's key is unique within its tenant, so the pair is unique too.
/// </summary>
public sealed class MediaFolderTests
{
    [Theory]
    [InlineData("demo", "demo")]
    [InlineData("Demo", "demo")]
    [InlineData("  demo  ", "demo")]
    [InlineData("cambridge-noida", "cambridge-noida")]
    [InlineData("Cambridge School Noida", "cambridge-school-noida")]
    public void AName_BecomesAReadableFolder(string name, string expected)
        => Assert.Equal(expected, MediaFolder.Segment(name, Guid.NewGuid()));

    /// <summary>
    /// This becomes part of an object's name in the bucket. A name must never be able to add a
    /// folder level of its own, or produce a key S3 will refuse.
    /// </summary>
    [Theory]
    [InlineData("../../etc", "etc")]
    [InlineData("a/b/c", "a-b-c")]
    [InlineData("school?x=1&y=2", "school-x-1-y-2")]
    [InlineData("../..", null)]
    [InlineData("///", null)]
    [InlineData("!!!", null)]
    public void AnUnsafeName_CannotEscapeItsOwnSegment(string name, string? expected)
    {
        var fallback = Guid.NewGuid();

        var segment = MediaFolder.Segment(name, fallback);

        Assert.DoesNotContain('/', segment);
        Assert.DoesNotContain("..", segment, StringComparison.Ordinal);
        Assert.Equal(expected ?? fallback.ToString(), segment);
    }

    /// <summary>
    /// A folder nobody can read is better than two schools sharing one, so a name that comes to
    /// nothing falls back to the id rather than to a word every site would share.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("-")]
    public void AMissingName_FallsBackToTheId(string? name)
    {
        var id = Guid.NewGuid();

        Assert.Equal(id.ToString(), MediaFolder.Segment(name, id));
    }

    [Fact]
    public void NoNameAndNoId_IsStillAValidSegment()
        => Assert.Equal("unknown", MediaFolder.Segment(null, null));

    /// <summary>A very long school name must not run away with the object's name.</summary>
    [Fact]
    public void AVeryLongName_IsCutShort()
    {
        var segment = MediaFolder.Segment(new string('a', 200), Guid.NewGuid());

        Assert.Equal(60, segment.Length);
    }

    /// <summary>
    /// The school's name is what somebody opening the bucket is looking for. Its key is
    /// guaranteed unique but says nothing — "school" and "college" name no school at all.
    /// </summary>
    [Fact]
    public void TheFolderIsNamedAfterTheSchool_NotItsKey()
    {
        var folder = MediaFolder.For(
            new FakeTenant("demo", Guid.NewGuid()),
            new FakeSite("school", "Cambridge High School", Guid.NewGuid()),
            "media");

        Assert.Equal("demo/cambridge-high-school/media", folder);
    }

    /// <summary>A website with no name still has a key, and a key always works.</summary>
    [Fact]
    public void WithNoName_TheKeyIsUsed()
    {
        var folder = MediaFolder.For(
            new FakeTenant("demo", Guid.NewGuid()),
            new FakeSite("junior-wing", "   ", Guid.NewGuid()),
            "media");

        Assert.Equal("demo/junior-wing/media", folder);
    }

    /// <summary>And with neither, the id — unreadable, but never another school's folder.</summary>
    [Fact]
    public void WithNeither_TheIdIsUsed()
    {
        var id = Guid.NewGuid();

        var folder = MediaFolder.For(
            new FakeTenant("demo", Guid.NewGuid()),
            new FakeSite("", null, id),
            "media");

        Assert.Equal($"demo/{id}/media", folder);
    }

    private sealed record FakeTenant(string? Code, Guid Id) : Cms.Application.Interfaces.ITenantContext
    {
        public Guid? TenantId => Id;
        public string? TenantCode => Code;
        public string? TenantName => Code;
        public bool IsResolved => true;
        public void Set(Guid tenantId, string code, string name) { }
    }

    private sealed record FakeSite(string? Key, string? Name, Guid Id) : Cms.Application.Interfaces.ISiteContext
    {
        public Guid? SiteId => Id;
        public string? SiteKey => Key;
        public string? SiteName => Name;
        public string BasePath => string.Empty;
        public bool IsResolved => true;
        public void Set(Guid siteId, string siteKey, string siteName, string basePath = "") { }
    }
}