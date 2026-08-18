using Cms.Application.Validators;

namespace Cms.Application.Tests;

/// <summary>
/// The operator's report: fill in the hero section, press save, and the editor answers
/// "$.description is reserved; use the section's standard field instead" — every time, on a
/// section whose configuration the application itself had written that way.
/// </summary>
public sealed class HomePageSectionConfigTests
{
    private const string SeededHero =
        """{"heading":"Welcome to Demo Academy","description":"Future Begins Here","primaryButton":"Apply Now"}""";

    [Fact]
    public void SeededHeroConfiguration_CanBeSaved()
    {
        var cleaned = HomePageSectionConfigValidator.StripReservedFields(SeededHero, out _);

        Assert.Empty(HomePageSectionConfigValidator.Validate("hero", cleaned));
    }

    [Fact]
    public void ReservedValue_IsHandedToTheFieldThatOwnsIt()
    {
        HomePageSectionConfigValidator.StripReservedFields(SeededHero, out var adopted);

        Assert.Equal("Future Begins Here", adopted["description"]);
    }

    [Fact]
    public void EverythingElse_IsLeftAlone()
    {
        var cleaned = HomePageSectionConfigValidator.StripReservedFields(SeededHero, out _);

        Assert.Contains("heading", cleaned);
        Assert.Contains("primaryButton", cleaned);
        Assert.DoesNotContain("description", cleaned);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("{}")]
    [InlineData("not json at all")]
    public void NothingToStrip_IsReturnedUntouched(string? json)
    {
        var cleaned = HomePageSectionConfigValidator.StripReservedFields(json, out var adopted);

        Assert.Equal(json, cleaned);
        Assert.Empty(adopted);
    }

    /// <summary>A reserved name nested inside a list is a normal field of that item.</summary>
    [Fact]
    public void ReservedNamesInsideItems_AreNotTouched()
    {
        const string json = """{"items":[{"title":"Science","imageUrl":"/img/a.png"}]}""";

        var cleaned = HomePageSectionConfigValidator.StripReservedFields(json, out var adopted);

        Assert.Empty(adopted);
        Assert.Contains("Science", cleaned);
        Assert.Empty(HomePageSectionConfigValidator.Validate("gallery", cleaned));
    }
}
