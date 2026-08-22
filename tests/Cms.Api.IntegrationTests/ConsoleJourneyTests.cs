extern alias adminapp;

using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.RegularExpressions;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// The journeys an operator actually performs, end to end, because every failure reported from
/// the field so far has been in one of them while the screens themselves rendered perfectly.
///
/// Rendering a page proves nothing: the top bar rendered while its menu could not open, the
/// picture picker rendered while it could not add a picture, and the hero editor rendered while
/// refusing every save. These walk the whole path — fill it in, save it, read it back.
/// </summary>
public sealed class ConsoleJourneyTests : IClassFixture<AdminFactory>
{
    private readonly HttpClient _client;

    public ConsoleJourneyTests(AdminFactory factory) =>
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true });

    /// <summary>Create a website, then edit it as itself rather than as somebody else's.</summary>
    [Fact]
    public async Task ANewWebsite_IsOfferedInTheTopBarAndStaysSelected()
    {
        await PostFormAsync("/CMS/Templates/Index", new Dictionary<string, string>
        {
            ["Input.TemplateKey"] = "metro-modern-school",
            ["Input.Name"] = "Riverside Public School",
            ["Input.SiteKey"] = "riverside-public",
            ["Input.IncludeSampleContent"] = "true"
        });

        var topBar = await GetAsync("/CMS/HomePage/Index");
        Assert.Contains("?site=riverside-public", topBar);

        var switched = await GetAsync("/CMS/HomePage/Index?site=riverside-public");
        Assert.Contains("Riverside Public School", CurrentWebsite(switched));

        // The choice has to survive the next page load, or every edit lands on the wrong site.
        var afterNavigating = await GetAsync("/CMS/Branding/Index");
        Assert.Contains("Riverside Public School", CurrentWebsite(afterNavigating));
    }

    /// <summary>
    /// The home banner. Its own seeded configuration carried a key the validator forbade, so
    /// this refused every save from the moment the website was created.
    /// </summary>
    [Fact]
    public async Task TheHomeBanner_AcceptsADescriptionAndKeepsIt()
    {
        const string words = "Education here goes well beyond textbooks and classrooms.";

        // The configuration a website created before the fix still carries: the key the
        // validator forbade is present, exactly as the seed and the templates used to write it.
        var saved = await PostFormAsync("/CMS/HomePage/Edit/hero", new Dictionary<string, string>
        {
            ["Input.Title"] = "Hero Banner",
            ["Input.SubTitle"] = "Banner Description",
            ["Input.Description"] = $"<p>{words}</p>",
            ["Input.JsonData"] =
                """{"heading":"Welcome","description":"Left over from an older site","primaryButton":"Apply Now"}""",
            ["Input.DisplayOrder"] = "1",
            ["IsActiveChecked"] = "true"
        });

        Assert.DoesNotContain("is reserved", saved);
        Assert.DoesNotContain("An unhandled exception", saved);
        Assert.Contains(words, await GetAsync("/CMS/HomePage/Edit/hero"));
    }

    /// <summary>The principal's name, designation and quote — entered, saved, and read back.</summary>
    [Fact]
    public async Task ThePrincipalsDetails_AreSavedAndComeBack()
    {
        await PostFormAsync("/CMS/HomePage/Edit/principal", new Dictionary<string, string>
        {
            ["Input.Title"] = "Principal Message",
            ["Input.JsonData"] =
                """{"personName":"Mrs. Sunita Sharma","designation":"Principal","quote":"Every child is known here."}""",
            ["Input.DisplayOrder"] = "4",
            ["IsActiveChecked"] = "true"
        });

        var reopened = await GetAsync("/CMS/HomePage/Edit/principal");
        Assert.Contains("Mrs. Sunita Sharma", reopened);
        Assert.Contains("Every child is known here.", reopened);
    }

    /// <summary>
    /// The principal's page in full: the message that is typed into the rich-text box, and the
    /// photograph attached to the section. Saving the details and saving the picture are two
    /// different paths through the same form, and only one of them had ever been checked.
    /// </summary>
    [Fact]
    public async Task ThePrincipalsMessageAndPhotograph_AreBothKept()
    {
        const string message = "Every child here is known by name, and expected to do their best.";

        var before = ImageOn(await GetAsync("/CMS/HomePage/Edit/principal"));

        var saved = await PostSectionWithImageAsync("/CMS/HomePage/Edit/principal", new Dictionary<string, string>
        {
            ["Input.Title"] = "Principal Message",
            ["Input.SubTitle"] = "From our Principal",
            ["Input.Description"] = $"<p>{message}</p>",
            ["Input.JsonData"] =
                """{"personName":"Mrs. Sunita Sharma","designation":"Principal","quote":"Known by name."}""",
            ["Input.DisplayOrder"] = "4",
            ["IsActiveChecked"] = "true"
        }, "principal.png");

        Assert.DoesNotContain("An unhandled exception", saved);

        var reopened = await GetAsync("/CMS/HomePage/Edit/principal");
        Assert.Contains(message, reopened);
        Assert.Contains("Mrs. Sunita Sharma", reopened);

        // The photograph must be stored against the section and actually reachable. Comparing
        // against what was there before, so a value the section already carried cannot make this
        // pass while the upload silently does nothing.
        var image = ImageOn(reopened);
        Assert.False(string.IsNullOrWhiteSpace(image), "The photograph was not kept on the section.");
        Assert.NotEqual(before, image);

        using var served = await _client.GetAsync(image);
        Assert.Equal(HttpStatusCode.OK, served.StatusCode);
    }

    /// <summary>
    /// Adding a picture on a new installation, where the library is empty. This is the path that
    /// dead-ended: the picker offered nothing to choose and no way to add anything.
    /// </summary>
    [Fact]
    public async Task APicture_CanBeAddedWhenTheLibraryIsEmpty()
    {
        var listedBefore = await GetAsync("/CMS/Media/Index?handler=List");

        var uploaded = await UploadAsync("/CMS/Media/Index?handler=Upload", "crest.png");
        Assert.NotEqual(HttpStatusCode.InternalServerError, uploaded);

        var listed = await GetAsync("/CMS/Media/Index?handler=List");
        Assert.Contains("crest.png", listed);
        Assert.DoesNotContain("crest.png", listedBefore);

        // The listed address must actually serve the file, or the picture is broken on the site.
        var url = Regex.Match(listed, "\"url\":\"([^\"]+)\"").Groups[1].Value.Replace("\\u0026", "&");
        Assert.False(string.IsNullOrWhiteSpace(url), "The uploaded file was listed without an address.");
        using var fetched = await _client.GetAsync(url);
        Assert.Equal(HttpStatusCode.OK, fetched.StatusCode);
    }

    [Fact]
    public async Task ALogo_ChosenForTheWebsite_IsKept()
    {
        await UploadAsync("/CMS/Media/Index?handler=Upload", "logo.png");
        var listed = await GetAsync("/CMS/Media/Index?handler=List");
        var url = Regex.Match(listed, "\"url\":\"([^\"]+)\"").Groups[1].Value;

        await PostFormAsync("/CMS/Branding/Index?handler=Save", new Dictionary<string, string>
        {
            ["Input.Name"] = "Cambridge High School",
            ["Input.LogoUrl"] = url,
            ["Input.PrimaryColor"] = "#0f2d5c",
            ["Input.SecondaryColor"] = "#c9a227"
        });

        Assert.Contains(url, await GetAsync("/CMS/Branding/Index"));
    }

    /// <summary>
    /// Nothing the console needs may come from the internet. The desktop build runs offline, and
    /// when the text editor was loaded from a CDN the description box became an empty area with
    /// no way to type into it and nothing to explain why.
    /// </summary>
    [Theory]
    [InlineData("/CMS/HomePage/Edit/hero")]
    [InlineData("/CMS/Branding/Index")]
    [InlineData("/CMS/Pages/Index")]
    [InlineData("/Account/ChangePassword")]
    public async Task NoScreen_DependsOnSomethingFetchedFromTheInternet(string path)
    {
        var html = await GetAsync(path);

        var external = Regex.Matches(html, @"<(?:script|link)[^>]*(?:src|href)=""(https?://[^""]+)""")
            .Select(m => m.Groups[1].Value)
            .ToList();

        Assert.True(external.Count == 0, $"{path} loads from outside: {string.Join(", ", external)}");
    }

    /// <summary>
    /// The console's own scripts must be served by the console and must still contain the pieces
    /// the journeys above cannot reach from the server: the picker's upload control, and the
    /// text editor itself. Both have been lost before, and both are invisible until somebody
    /// clicks the thing on a machine with no internet.
    /// </summary>
    [Theory]
    [InlineData("/js/cms-media-picker.js", "data-picker-upload")]
    [InlineData("/js/homepage.js", "data-dropdown-trigger")]
    [InlineData("/js/homepage.js", "editor-fallback")]
    [InlineData("/lib/quill/quill.js", "Quill")]
    [InlineData("/css/admin.css", ".media-picker__upload")]
    public async Task TheConsolesOwnAssets_AreServedAndComplete(string path, string mustContain)
    {
        using var response = await _client.GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(mustContain, await response.Content.ReadAsStringAsync());
    }

    /// <summary>Submits a section form the way the browser does — as a file upload.</summary>
    private async Task<string> PostSectionWithImageAsync(
        string path, Dictionary<string, string> changes, string fileName)
    {
        var page = await GetAsync(path);
        var values = FieldsFrom(page);
        foreach (var change in changes) values[change.Key] = change.Value;
        values["__RequestVerificationToken"] = TokenFrom(page);

        var form = new MultipartFormDataContent();
        foreach (var value in values)
        {
            form.Add(new StringContent(value.Value), value.Key);
        }

        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");
        var file = new ByteArrayContent(png);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        form.Add(file, "imageFile", fileName);

        using var response = await _client.PostAsync(path, form);
        return await response.Content.ReadAsStringAsync();
    }

    private static string ImageOn(string html) =>
        Regex.Match(html, @"name=""Input\.ImageUrl""[^>]*value=""([^""]*)""").Groups[1].Value;

    private static string CurrentWebsite(string html) =>
        Regex.Match(html, @"site-switcher__trigger.*?<strong>(.*?)</strong>", RegexOptions.Singleline).Value;

    private async Task<string> GetAsync(string path)
    {
        using var response = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Submits the form the page actually rendered, carrying every field it contains and
    /// changing only what the test is about. Posting a handful of hand-picked fields instead
    /// would fail validation for reasons no real operator would ever hit.
    /// </summary>
    private async Task<string> PostFormAsync(string path, Dictionary<string, string> changes)
    {
        var page = await GetAsync(path.Split('?')[0]);
        var values = FieldsFrom(page);

        foreach (var change in changes)
        {
            values[change.Key] = change.Value;
        }

        values["__RequestVerificationToken"] = TokenFrom(page);

        using var response = await _client.PostAsync(path, new FormUrlEncodedContent(values));
        var html = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain("An unhandled exception occurred", html);
        return html;
    }

    /// <summary>Every named value the rendered page would submit.</summary>
    private static Dictionary<string, string> FieldsFrom(string html)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (Match input in Regex.Matches(html, "<input\\b[^>]*>"))
        {
            var name = Attribute(input.Value, "name");
            if (name is null || name == "__RequestVerificationToken") continue;

            var type = Attribute(input.Value, "type") ?? "text";
            if (type is "checkbox" or "radio")
            {
                if (input.Value.Contains("checked", StringComparison.OrdinalIgnoreCase))
                {
                    values[name] = Attribute(input.Value, "value") ?? "true";
                }
                continue;
            }

            if (type == "file") continue;
            values[name] = WebUtility.HtmlDecode(Attribute(input.Value, "value") ?? string.Empty);
        }

        foreach (Match area in Regex.Matches(html, "<textarea\\b[^>]*>(.*?)</textarea>", RegexOptions.Singleline))
        {
            var name = Attribute(area.Value, "name");
            if (name is not null) values[name] = WebUtility.HtmlDecode(area.Groups[1].Value);
        }

        foreach (Match select in Regex.Matches(html, "<select\\b[^>]*>.*?</select>", RegexOptions.Singleline))
        {
            var name = Attribute(select.Value, "name");
            if (name is null) continue;

            var chosen = Regex.Match(select.Value, "<option[^>]*\\bselected\\b[^>]*>", RegexOptions.IgnoreCase);
            var option = chosen.Success
                ? chosen.Value
                : Regex.Match(select.Value, "<option[^>]*>").Value;
            values[name] = WebUtility.HtmlDecode(Attribute(option, "value") ?? string.Empty);
        }

        return values;
    }

    private static string? Attribute(string tag, string name)
    {
        var match = Regex.Match(tag, name + "=\"([^\"]*)\"", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value : null;
    }

    private async Task<HttpStatusCode> UploadAsync(string path, string fileName)
    {
        var page = await GetAsync("/CMS/Media/Index");

        // A one-pixel PNG: real bytes, so the image pipeline is genuinely exercised.
        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

        var file = new ByteArrayContent(png);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/png");

        var form = new MultipartFormDataContent
        {
            { new StringContent(TokenFrom(page)), "__RequestVerificationToken" },
            { new StringContent("image"), "UploadKind" },
            { file, "Upload", fileName }
        };

        using var response = await _client.PostAsync(path, form);
        return response.StatusCode;
    }

    private static string TokenFrom(string html)
    {
        var token = Regex.Match(html, @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""").Groups[1].Value;
        Assert.False(string.IsNullOrEmpty(token), "The page carried no antiforgery token.");
        return token;
    }
}
