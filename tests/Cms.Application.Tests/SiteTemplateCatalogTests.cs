using Cms.Application.Templates;

namespace Cms.Application.Tests;

/// <summary>
/// Templates are shown to prospective schools, so a broken one is visible to a customer
/// rather than to us. These guard the catalog's shape.
/// </summary>
public sealed class SiteTemplateCatalogTests
{
    [Fact]
    public void EveryTemplate_IsCompleteEnoughToShow()
    {
        Assert.NotEmpty(SiteTemplateCatalog.All);

        foreach (var template in SiteTemplateCatalog.All)
        {
            Assert.False(string.IsNullOrWhiteSpace(template.Name), $"{template.Key} has no name");
            Assert.False(string.IsNullOrWhiteSpace(template.Summary), $"{template.Key} has no summary");
            Assert.False(string.IsNullOrWhiteSpace(template.BestFor), $"{template.Key} has no guidance");
            Assert.False(string.IsNullOrWhiteSpace(template.HeroHeading), $"{template.Key} has no hero heading");

            Assert.NotEmpty(template.Highlights);
            Assert.NotEmpty(template.PageTemplateKeys);

            // A template exists to look finished; empty sample content defeats the purpose.
            Assert.NotEmpty(template.Faculty);
            Assert.NotEmpty(template.Departments);
            Assert.NotEmpty(template.News);
            Assert.NotEmpty(template.Events);

            Assert.StartsWith("#", template.PrimaryColor);
            Assert.StartsWith("#", template.SecondaryColor);
        }
    }

    [Fact]
    public void TemplateKeys_AreUnique()
    {
        var keys = SiteTemplateCatalog.All.Select(x => x.Key).ToList();
        Assert.Equal(keys.Count, keys.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void EveryHomeDesign_IsRepresentedByATemplate()
    {
        var covered = SiteTemplateCatalog.All.Select(x => x.HomeVariant).Distinct().ToList();
        Assert.Equal(Enum.GetValues<Cms.Domain.Enums.HomeVariant>().Length, covered.Count);
    }

    [Fact]
    public void EachTemplate_HasExactlyOneFeaturedNoticeAtMost()
    {
        foreach (var template in SiteTemplateCatalog.All)
        {
            Assert.True(template.News.Count(x => x.IsFeatured) <= 1,
                $"{template.Key} pins more than one notice to the top of the news page");
        }
    }

    [Fact]
    public void EachTemplate_MixesUpcomingAndPastEvents()
    {
        foreach (var template in SiteTemplateCatalog.All)
        {
            Assert.Contains(template.Events, x => x.DaysFromNow > 0);
        }
    }

    [Fact]
    public void Find_IsCaseInsensitiveAndReturnsNullForUnknown()
    {
        Assert.NotNull(SiteTemplateCatalog.Find("HERITAGE-DAY-SCHOOL"));
        Assert.Null(SiteTemplateCatalog.Find("no-such-template"));
    }
}
