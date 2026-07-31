using Cms.Application.Mapping;
using Cms.Domain.Entities;

namespace Cms.Application.Tests;

public sealed class HomePageMapperTests
{
    [Fact]
    public void ToFrontendSection_DoesNotAllowJsonToOverrideCanonicalFields()
    {
        var section = new HomePageSection
        {
            SectionKey = "hero",
            Title = "Trusted title",
            DisplayOrder = 3,
            IsActive = true,
            JsonData = """{"title":"Injected title","displayOrder":999,"isActive":false,"heading":"Welcome"}"""
        };

        var result = HomePageMapper.ToFrontendSection(section);

        Assert.Equal("Trusted title", result["title"]);
        Assert.Equal(3, result["displayOrder"]);
        Assert.Equal(true, result["isActive"]);
        Assert.Equal("Welcome", result["heading"]?.ToString());
    }

    [Fact]
    public void ToFrontendSection_ExpandsSectionSpecificConfiguration()
    {
        var section = new HomePageSection
        {
            SectionKey = "statistics",
            Title = "Our impact",
            JsonData = """{"students":1500,"teachers":80}"""
        };

        var result = HomePageMapper.ToFrontendSection(section);

        Assert.Equal("1500", result["students"]?.ToString());
        Assert.Equal("80", result["teachers"]?.ToString());
    }
}
