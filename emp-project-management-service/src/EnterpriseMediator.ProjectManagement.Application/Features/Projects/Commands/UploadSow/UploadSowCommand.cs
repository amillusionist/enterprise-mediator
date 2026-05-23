using EnterpriseMediator.ProjectManagement.Application.Common;
using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.UploadSow;

public record UploadSowCommand(
    Guid ProjectId,
    Stream FileStream,
    string FileName,
    string ContentType) : IRequest<Result<Guid>>;
