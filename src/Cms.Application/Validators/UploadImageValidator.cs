using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Cms.Application.Validators;

public class UploadImageValidator : AbstractValidator<IFormFile>
{
    public static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/webp",
        "image/gif"
    ];

    public const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public UploadImageValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .NotNull()
            .WithMessage("Image file is required.");

        RuleFor(x => x.Length)
            .GreaterThan(0)
            .WithMessage("Image file is empty.")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage($"Image must be {MaxFileSizeBytes / (1024 * 1024)} MB or smaller.");

        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Only JPEG, PNG, WEBP, and GIF images are allowed.");

        RuleFor(x => x)
            .Must(HaveValidImageSignature)
            .WithMessage("The file content does not match a supported image format.");
    }

    private static bool HaveValidImageSignature(IFormFile file)
    {
        Span<byte> header = stackalloc byte[12];
        using var stream = file.OpenReadStream();
        var bytesRead = stream.Read(header);
        if (bytesRead < 6)
        {
            return false;
        }

        var isJpeg = header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;
        var isPng = bytesRead >= 8
            && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        var isGif = header[..6].SequenceEqual("GIF87a"u8) || header[..6].SequenceEqual("GIF89a"u8);
        var isWebP = bytesRead >= 12
            && header[..4].SequenceEqual("RIFF"u8)
            && header.Slice(8, 4).SequenceEqual("WEBP"u8);

        return file.ContentType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => isJpeg,
            "image/png" => isPng,
            "image/gif" => isGif,
            "image/webp" => isWebP,
            _ => false
        };
    }
}
