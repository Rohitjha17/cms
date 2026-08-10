using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Cms.Application.Validators;

public sealed class UploadVideoValidator : AbstractValidator<IFormFile>
{
    public const long MaxFileSizeBytes = 100 * 1024 * 1024;

    public UploadVideoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x).NotNull().WithMessage("Video file is required.");
        RuleFor(x => x.Length)
            .GreaterThan(0).WithMessage("Video file is empty.")
            .LessThanOrEqualTo(MaxFileSizeBytes).WithMessage("Video must be 100 MB or smaller.");
        RuleFor(x => x.ContentType)
            .Must(x => x.Equals("video/mp4", StringComparison.OrdinalIgnoreCase)
                || x.Equals("video/webm", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Only MP4 and WebM videos are allowed.");
        RuleFor(x => x).Must(HaveValidSignature)
            .WithMessage("The file content does not match a supported video format.");
    }

    private static bool HaveValidSignature(IFormFile file)
    {
        Span<byte> header = stackalloc byte[12];
        using var stream = file.OpenReadStream();
        var read = stream.Read(header);
        if (read < 4) return false;

        var isMp4 = read >= 12 && header.Slice(4, 4).SequenceEqual("ftyp"u8);
        var isWebM = header[..4].SequenceEqual(new byte[] { 0x1A, 0x45, 0xDF, 0xA3 });
        return file.ContentType.Equals("video/mp4", StringComparison.OrdinalIgnoreCase) ? isMp4 : isWebM;
    }
}
