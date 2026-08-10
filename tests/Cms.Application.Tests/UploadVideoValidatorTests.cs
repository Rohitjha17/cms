using Cms.Application.Validators;
using Microsoft.AspNetCore.Http;

namespace Cms.Application.Tests;

public sealed class UploadVideoValidatorTests
{
    [Fact]
    public async Task RejectsSpoofedMp4ContentType()
    {
        var bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        var file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "fake.mp4")
        {
            Headers = new HeaderDictionary(),
            ContentType = "video/mp4"
        };

        var result = await new UploadVideoValidator().ValidateAsync(file);
        Assert.False(result.IsValid);
    }
}
