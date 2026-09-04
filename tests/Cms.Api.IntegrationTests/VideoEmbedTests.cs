extern alias webapp;

using VideoEmbed = webapp::Cms.Web.Helpers.VideoEmbed;

namespace Cms.Api.IntegrationTests;

/// <summary>
/// What a school pastes into the video field is whatever their address bar had in it. Every
/// shape below is one a person really copies. A link this cannot read must come back as
/// nothing, because an unknown address built into an iframe is somebody else's page running
/// inside the school's.
/// </summary>
public sealed class VideoEmbedTests
{
    [Theory]
    [InlineData("https://www.youtube.com/watch?v=dQw4w9WgXcQ")]
    [InlineData("https://youtube.com/watch?v=dQw4w9WgXcQ&t=42")]
    [InlineData("https://youtu.be/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/embed/dQw4w9WgXcQ")]
    [InlineData("https://www.youtube.com/shorts/dQw4w9WgXcQ")]
    [InlineData("https://m.youtube.com/watch?v=dQw4w9WgXcQ")]
    public void EveryShapeOfYouTubeLink_FindsTheSameFilm(string link)
    {
        var video = VideoEmbed.Read(link);

        Assert.NotNull(video);
        Assert.Equal("https://www.youtube-nocookie.com/embed/dQw4w9WgXcQ?rel=0", video.EmbedUrl);
        Assert.Equal("https://i.ytimg.com/vi/dQw4w9WgXcQ/hqdefault.jpg", video.PosterUrl);
    }

    [Fact]
    public void Vimeo_BecomesItsPlayer()
    {
        Assert.Equal(
            "https://player.vimeo.com/video/76979871",
            VideoEmbed.Read("https://vimeo.com/76979871")!.EmbedUrl);
    }

    [Theory]
    [InlineData("https://cdn.example.org/tour.mp4")]
    [InlineData("/uploads/WebSiteData/annual-day.webm")]
    public void AnUploadedFile_PlaysDirectlyRatherThanInAFrame(string link)
    {
        var video = VideoEmbed.Read(link);

        Assert.NotNull(video);
        Assert.Null(video.EmbedUrl);
        Assert.Equal(link, video.FileUrl);
    }

    /// <summary>
    /// The important half: a link this cannot read is not guessed at and not passed through.
    /// </summary>
    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html;base64,PHNjcmlwdD4=")]
    [InlineData("https://evil.example.com/watch?v=abc")]
    [InlineData("https://youtube.com.evil.example/watch?v=abc")]
    [InlineData("https://www.youtube.com/watch?v=../../etc/passwd")]
    [InlineData("https://vimeo.com/not-a-number")]
    [InlineData("https://example.org/page.html")]
    [InlineData("uploads/relative-without-slash.mp4")]
    [InlineData("")]
    [InlineData(null)]
    public void ALinkItCannotRead_IsNothing(string? link)
    {
        Assert.Null(VideoEmbed.Read(link));
    }

    [Fact]
    public void AYouTubeIdIsOnlyEverIdCharacters()
    {
        Assert.Null(VideoEmbed.Read("https://www.youtube.com/watch?v=\"><script>alert(1)</script>"));
    }
}
