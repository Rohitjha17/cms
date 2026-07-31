using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Cms.Application.Validators;

public sealed class UploadDocumentValidator : AbstractValidator<IFormFile>
{
    public const long MaxFileSizeBytes = 15 * 1024 * 1024;

    public UploadDocumentValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x)
            .NotNull()
            .WithMessage("Document file is required.");

        RuleFor(x => x.Length)
            .GreaterThan(0)
            .WithMessage("Document file is empty.")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage("Document must be 15 MB or smaller.");

        RuleFor(x => x.ContentType)
            .Equal("application/pdf", StringComparer.OrdinalIgnoreCase)
            .WithMessage("Only PDF documents are allowed.");

        RuleFor(x => x)
            .Must(HavePdfSignature)
            .WithMessage("The file content is not a valid PDF document.");
    }

    private static bool HavePdfSignature(IFormFile file)
    {
        Span<byte> header = stackalloc byte[5];
        using var stream = file.OpenReadStream();
        return stream.Read(header) == header.Length && header.SequenceEqual("%PDF-"u8);
    }
}
