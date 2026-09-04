extern alias webapp;

using MapEmbed = webapp::Cms.Web.Helpers.MapEmbed;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// Every one of these is something a person really does with the map field. Only the first was
/// ever handled; the rest drew an empty grey box, because Google refuses to let its ordinary
/// map pages be framed and nothing on the page said so.
/// </summary>
public sealed class MapEmbedTests
{
    [Fact]
    public void TheEmbedAddressFromGoogle_IsKeptExactlyAsItIs()
    {
        const string embed = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3502";

        Assert.Equal(embed, MapEmbed.Read(embed));
    }

    /// <summary>The commonest mistake: copying the whole tag instead of the address in it.</summary>
    [Fact]
    public void TheWholeIframeTag_HasItsAddressTakenOut()
    {
        var pasted = """<iframe src="https://www.google.com/maps/embed?pb=!1m18!ABC" width="600" height="450" style="border:0;" allowfullscreen loading="lazy"></iframe>""";

        Assert.Equal("https://www.google.com/maps/embed?pb=!1m18!ABC", MapEmbed.Read(pasted));
    }

    /// <summary>What is in the address bar while you are looking at the school.</summary>
    [Fact]
    public void ALinkWithTheMapsCentreInIt_BecomesAMapOfThatPoint()
    {
        var link = "https://www.google.com/maps/place/Cambridge+School/@28.5709,77.3260,17z/data=!3m1";

        var map = MapEmbed.Read(link);

        Assert.Contains("output=embed", map);
        Assert.Contains("28.5709%2C77.3260", map);
        Assert.Contains("z=17", map);
    }

    [Fact]
    public void ALinkNamingThePlace_BecomesAMapOfThatName()
    {
        var map = MapEmbed.Read("https://www.google.com/maps/place/Cambridge+School+Noida");

        Assert.Contains("output=embed", map);
        Assert.Contains("Cambridge%20School%20Noida", map);
    }

    [Fact]
    public void ASearchLink_BecomesAMapOfWhatWasSearchedFor()
    {
        var map = MapEmbed.Read("https://maps.google.com/maps?q=Cambridge+School+Sector+27+Noida");

        Assert.Contains("Cambridge%20School%20Sector%2027%20Noida", map);
    }

    /// <summary>Not a link at all — the school simply typed where it is.</summary>
    [Fact]
    public void PlainAddressText_IsLookedUp()
    {
        var map = MapEmbed.Read("Cambridge School, Sector-27, Noida, UP-201301");

        Assert.StartsWith("https://maps.google.com/maps?output=embed&q=", map);
        Assert.Contains("Sector-27", map);
    }

    /// <summary>
    /// A short link cannot be expanded without asking Google, and a page must not make an
    /// outbound request to draw itself. The school's own address finds the same place.
    /// </summary>
    [Theory]
    [InlineData("https://maps.app.goo.gl/AbCdEf123")]
    [InlineData("https://goo.gl/maps/AbCdEf123")]
    public void AShortLink_FallsBackToTheSchoolsAddress(string link)
    {
        var map = MapEmbed.Read(link, "Sector-27, Noida");

        Assert.Contains("Sector-27%2C%20Noida", map);
    }

    [Fact]
    public void NothingInTheFieldAtAll_StillMapsTheAddress()
    {
        Assert.Contains("Sector-27", MapEmbed.Read(null, "Sector-27, Noida"));
    }

    [Fact]
    public void NothingAnywhere_IsNoMapRatherThanAnEmptyFrame()
    {
        Assert.Null(MapEmbed.Read(null, null));
        Assert.Null(MapEmbed.Read("   ", "  "));
    }

    /// <summary>
    /// This value becomes an iframe's source. Whatever is typed, what comes out must be a
    /// Google Maps address built here — never the text passed through.
    /// </summary>
    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("https://evil.example.com/page")]
    [InlineData("""<iframe src="https://evil.example.com/x"></iframe>""")]
    [InlineData("https://google.com.evil.example/maps/place/X")]
    public void AnythingElse_NeverBecomesTheFrameItself(string value)
    {
        var map = MapEmbed.Read(value, "Sector-27, Noida");

        Assert.NotNull(map);
        Assert.StartsWith("https://maps.google.com/maps?output=embed&q=", map);
        Assert.DoesNotContain("evil.example", map, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("javascript:", map, StringComparison.OrdinalIgnoreCase);
    }
}
