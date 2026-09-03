extern alias webapp;

using AppearanceChoice = webapp::Cms.Web.Helpers.AppearanceChoice;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// These choices become class names and a colour in the page's own markup, from values typed
/// into the console. A name that is not one of ours must become nothing at all rather than be
/// passed through and hoped for.
/// </summary>
public sealed class AppearanceChoiceTests
{
    [Theory]
    [InlineData("solid", "btn-style-solid")]
    [InlineData("GRADIENT", "btn-style-gradient")]
    [InlineData("  outline  ", "btn-style-outline")]
    public void AKnownStyle_BecomesItsClass(string value, string expected)
        => Assert.Equal(expected, AppearanceChoice.ButtonStyle(value));

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("neon")]
    [InlineData("solid\" onload=\"alert(1)")]
    public void AnythingElse_BecomesNoClass(string? value)
    {
        Assert.Null(AppearanceChoice.ButtonStyle(value));
        Assert.Null(AppearanceChoice.ButtonShape(value));
        Assert.Null(AppearanceChoice.ButtonHover(value));
        Assert.Null(AppearanceChoice.CardHover(value));
    }

    [Theory]
    [InlineData("#c0392b", "#c0392b")]
    [InlineData("#FFF", "#FFF")]
    [InlineData("  #0f2d5c  ", "#0f2d5c")]
    public void AHexColour_IsKept(string value, string expected)
        => Assert.Equal(expected, AppearanceChoice.Color(value));

    /// <summary>The colour goes into a style attribute, so it may only ever be a colour.</summary>
    [Theory]
    [InlineData("red")]
    [InlineData("#12345")]
    [InlineData("#gggggg")]
    [InlineData("red;background-image:url(http://x/y)")]
    [InlineData("#fff\" onmouseover=\"alert(1)")]
    [InlineData(null)]
    public void AnythingThatIsNotAHexColour_IsRefused(string? value)
        => Assert.Null(AppearanceChoice.Color(value));
}
