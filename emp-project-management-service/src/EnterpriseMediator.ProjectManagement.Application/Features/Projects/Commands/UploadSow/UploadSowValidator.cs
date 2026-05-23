using FluentValidation;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.UploadSow;

public class UploadSowValidator : AbstractValidator<UploadSowCommand>
{
    private static readonly string[] AllowedContentTypes = { "application/pdf", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/msword" };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB

    public UploadSowValidator()
    {
        RuleFor(x => x.ProjectId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.FileStream).NotNull().Must(s => s.Length > 0).WithMessage("File cannot be empty.")
            .Must(s => s.Length <= MaxFileSizeBytes).WithMessage("File size must not exceed 10MB.");
        RuleFor(x => x.ContentType).NotEmpty()
            .Must(ct => AllowedContentTypes.Contains(ct)).WithMessage("Only PDF, DOCX, and DOC files are allowed.");
    }
}
