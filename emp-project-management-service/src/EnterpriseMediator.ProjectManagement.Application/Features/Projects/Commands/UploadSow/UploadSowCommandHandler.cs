using EnterpriseMediator.ProjectManagement.Application.Common;
using EnterpriseMediator.ProjectManagement.Application.Interfaces;
using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.UploadSow;

public class UploadSowCommandHandler : IRequestHandler<UploadSowCommand, Result<Guid>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<UploadSowCommandHandler> _logger;

    public UploadSowCommandHandler(
        IProjectRepository projectRepository,
        IFileStorageService fileStorage,
        ILogger<UploadSowCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(UploadSowCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Uploading SOW for project {ProjectId}, file {FileName}", request.ProjectId, request.FileName);

        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null)
            return Result<Guid>.Failure("Project not found.");

        var storageKey = await _fileStorage.UploadAsync(request.FileStream, request.FileName, request.ContentType, cancellationToken);

        var sowDoc = project.UploadSow(request.FileName, request.ContentType, request.FileStream.Length, storageKey, Guid.Empty);

        _projectRepository.Update(project);
        await _projectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("SOW {SowId} uploaded for project {ProjectId}", sowDoc.Id, request.ProjectId);
        return Result<Guid>.Success(sowDoc.Id);
    }
}
