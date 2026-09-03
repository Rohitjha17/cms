extern alias webapp;

using SectionBackdrop = webapp::Cms.Web.Helpers.SectionBackdrop;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// A section's backdrop is chosen in the console and ends up in a stylesheet, so the value
/// crosses from something a school typed into something the browser executes. A quote or a
/// brace would close the rule early and let whatever follows through, so the address is refused
/// rather than repaired, and the pattern can only be one of the ones we drew.
/// </summary>
public sealed class SectionBackdropTests
{
    [Theory]
    [InlineData("/uploads/WebSiteData/a/b/media/hall.jpg")]
    [InlineData("https://cdn.school.edu.in/hall.jpg?v=2")]
    public void AnOrdinaryAddress_IsKept(string url)
        => Assert.Equal(url, SectionBackdrop.SafeUrl(url));

    [Theory]
    [InlineData("a.jpg\") ;} body{display:none} .x{background:url(\"b.jpg")]
    [InlineData("a.jpg') ;} html{opacity:0}")]
    [InlineData("a.jpg\n}\nbody{display:none}")]
    [InlineData("/*x*/a.jpg")]
    [InlineData("javascript:alert(1)")]
    public void AnAddressThatCouldEscapeTheRule_IsRefused(string url)
        => Assert.Equal(string.Empty, SectionBackdrop.SafeUrl(url));

    [Fact]
    public void EveryOfferedPattern_ResolvesToDrawableCss()
    {
        foreach (var (name, style) in SectionBackdrop.Patterns)
        {
            Assert.False(string.IsNullOrWhiteSpace(style.Image), $"{name} draws nothing.");
            Assert.False(string.IsNullOrWhiteSpace(style.Size), $"{name} has no tile size.");
            // Drawn, never downloaded: a decorative backdrop that costs a request and a
            // megabyte is a backdrop that makes the page worse.
            Assert.DoesNotContain("url(", style.Image);
            Assert.Same(style, SectionBackdrop.Style(name));
        }
    }

    [Fact]
    public void TheMovingOnes_NameAKeyframeAndNothingElse()
    {
        var moving = SectionBackdrop.Patterns.Values.Where(x => x.Animation is not null).ToList();
        Assert.NotEmpty(moving);

        foreach (var style in moving)
        {
            // An animation shorthand ends up inside a rule this application writes; a semicolon
            // or a brace in it would close that rule early.
            Assert.DoesNotContain(";", style.Animation);
            Assert.DoesNotContain("}", style.Animation);
            Assert.StartsWith("backdrop-", style.Animation);
        }
    }

    [Theory]
    [InlineData("stripes-that-do-not-exist")]
    [InlineData("red;} body{display:none")]
    [InlineData(null)]
    public void AnythingElse_IsNoBackdropAtAll(string? name)
        => Assert.Null(SectionBackdrop.Style(name));
}
