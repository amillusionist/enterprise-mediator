using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Projects.Commands
{
    /// <summary>
    /// Handles the CreateProjectCommand by forwarding the request to the Project Microservice.
    /// </summary>
    public class CreateProjectHandler : IRequestHandler<CreateProjectCommand, Guid>
    {
        private readonly IProjectServiceClient _projectService;
        private readonly ILogger<CreateProjectHandler> _logger;

        public CreateProjectHandler(
            IProjectServiceClient projectService,
            ILogger<CreateProjectHandler> logger)
        {
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling CreateProjectCommand for project: {ProjectName}", request.Name);

            var dto = new CreateProjectDto(
                request.Name,
                request.Description,
                request.ClientId,
                request.StartDate,
                request.EndDate);

            var projectId = await _projectService.CreateProjectAsync(dto, cancellationToken);

            _logger.LogInformation("Project created with ID: {ProjectId}", projectId);

            return projectId;
        }
    }
}
