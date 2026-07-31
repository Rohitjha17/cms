using Cms.Application.DTOs.HomePage;
using Cms.Application.Validators;
using Microsoft.AspNetCore.Http;

namespace Cms.Application.Tests;

public sealed class HomePageValidatorTests
{
    [Fact]
    public async Task UpdateValidator_RejectsInvalidJsonAndUnsafeUrl()
    {
        var validator = new UpdateHomePageSectionValidator();
        var result = await validator.ValidateAsync(new UpdateHomePageSectionDto
        {
            Title = "Hero",
            ButtonLink = "javascript:alert(1)",
            JsonData = "{broken"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(UpdateHomePageSectionDto.ButtonLink));
        Assert.Contains(result.Errors, x => x.PropertyName == nameof(UpdateHomePageSectionDto.JsonData));
    }

    [Fact]
    public async Task UploadValidator_RejectsSpoofedImageContentType()
    {
        var bytes = "this is not an image"u8.ToArray();
        await using var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "file", "fake.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var result = await new UploadImageValidator().ValidateAsync(file);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, x => x.ErrorMessage.Contains("file content"));
    }

    [Fact]
    public async Task ReorderValidator_RejectsDuplicateKeysAndOrders()
    {
        var validator = new ReorderHomePageSectionsValidator();
        var result = await validator.ValidateAsync(new ReorderHomePageSectionsDto
        {
            Items =
            [
                new ReorderItemDto { SectionKey = "hero", DisplayOrder = 1 },
                new ReorderItemDto { SectionKey = "hero", DisplayOrder = 1 }
            ]
        });

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);
    }

    [Fact]
    public void SectionConfigValidator_RejectsReservedFieldsAndUnsafeNestedUrls()
    {
        var errors = HomePageSectionConfigValidator.Validate(
            "gallery",
            """{"title":"Override","items":[{"imageUrl":"javascript:alert(1)"}]}""");

        Assert.Contains(errors, x => x.Contains("$.title"));
        Assert.Contains(errors, x => x.Contains("imageUrl"));
    }

    [Fact]
    public async Task DocumentValidator_AcceptsPdfSignature()
    {
        var bytes = "%PDF-1.7 test"u8.ToArray();
        await using var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "file", "prospectus.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        var result = await new UploadDocumentValidator().ValidateAsync(file);

        Assert.True(result.IsValid);
    }
}
