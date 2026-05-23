using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Projects.Commands
{
    /// <summary>
    /// Handles the UploadSowCommand by forwarding the SOW document to the Project Microservice.
    /// </summary>
    public class UploadSowHandler : IRequestHandler<UploadSowCommand, Unit>
    {
        private readonly IProjectServiceClient _projectService;
        private readonly ILogger<UploadSowHandler> _logger;

        public UploadSowHandler(
            IProjectServiceClient projectService,
            ILogger<UploadSowHandler> logger)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(UploadSowCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Handling UploadSowCommand for ProjectId: {ProjectId}, FileName: {FileName}",
                request.ProjectId,
                request.FileName);

            await _projectService.UploadSowAsync(
                request.ProjectId,
                request.FileStream,
                request.FileName,
                request.ContentType,
                cancellationToken);

            _logger.LogInformation("SOW upload completed for ProjectId: {ProjectId}", request.ProjectId);

            return Unit.Value;
        }
    }
}
