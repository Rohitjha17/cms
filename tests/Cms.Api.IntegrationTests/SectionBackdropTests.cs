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
    public void EveryOfferedPattern_ResolvesToCss()
    {
        foreach (var name in SectionBackdrop.Patterns.Keys)
        {
            Assert.NotEqual("none", SectionBackdrop.Pattern(name));
        }
    }

    [Theory]
    [InlineData("stripes-that-do-not-exist")]
    [InlineData("red;} body{display:none")]
    [InlineData(null)]
    public void AnythingElse_IsNoPatternAtAll(string? name)
        => Assert.Equal("none", SectionBackdrop.Pattern(name));
}
