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

            // Two schools created from different templates must not open on the same photograph.
            Assert.False(string.IsNullOrWhiteSpace(template.HeroImageUrl), $"{template.Key} has no hero image");
            Assert.StartsWith("https://", template.HeroImageUrl);
        }
    }

    [Fact]
    public void HeroImages_AreDistinctPerTemplate()
    {
        var images = SiteTemplateCatalog.All.Select(x => x.HeroImageUrl).ToList();

        Assert.Equal(images.Count, images.Distinct().Count());
    }

    /// <summary>
    /// A template that promises a moving hero has to ship the pictures for it, or the school
    /// gets a still hero and a promise the gallery made on its behalf.
    /// </summary>
    [Fact]
    public void ATemplatePromisingASlideshow_ShipsTheePictures()
    {
        foreach (var template in SiteTemplateCatalog.All)
        {
            var promisesSlideshow = template.Highlights
                .Any(h => h.Contains("slideshow", StringComparison.OrdinalIgnoreCase));

            if (!promisesSlideshow)
            {
                continue;
            }

            Assert.True(
                template.HeroImages.Count > 1,
                $"{template.Key} advertises a slideshow but ships {template.HeroImages.Count} picture(s).");
            Assert.All(template.HeroImages, image => Assert.StartsWith("https://", image));
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

    /// <summary>
    /// A template exists so a site can be shown to a school as-is. Placeholder copy such as
    /// "edit this page" defeats that, so every template must supply finished text for the
    /// pages a visitor actually reads.
    /// </summary>
    [Fact]
    public void EveryTemplate_ShipsFinishedPageCopy()
    {
        string[] mustHave = ["about", "admission", "facilities", "messages", "committee"];

        foreach (var template in SiteTemplateCatalog.All)
        {
            foreach (var page in mustHave)
            {
                Assert.True(template.PageContent.ContainsKey(page),
                    $"{template.Key} has no copy for the {page} page");

                var copy = template.PageContent[page];
                Assert.True(copy.Length > 300, $"{template.Key}/{page} copy is too thin to show a school");
                Assert.DoesNotContain("Edit this page", copy, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("Lorem", copy, StringComparison.OrdinalIgnoreCase);
            }

            // The school's own name must appear, so the site does not read as a generic demo.
            Assert.Contains(template.PageContent.Values, x => x.Contains("{name}"));
        }
    }

    [Fact]
    public void Find_IsCaseInsensitiveAndReturnsNullForUnknown()
    {
        Assert.NotNull(SiteTemplateCatalog.Find("HERITAGE-DAY-SCHOOL"));
        Assert.Null(SiteTemplateCatalog.Find("no-such-template"));
    }
}
