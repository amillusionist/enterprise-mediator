using EnterpriseMediator.ProjectManagement.Application.Common;
using EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;
using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Milestones.Commands.AddMilestone;

public class AddMilestoneCommandHandler : IRequestHandler<AddMilestoneCommand, Result<Guid>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<AddMilestoneCommandHandler> _logger;

    public AddMilestoneCommandHandler(IProjectRepository projectRepository, ILogger<AddMilestoneCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<Result<Guid>> Handle(AddMilestoneCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdWithMilestonesAsync(request.ProjectId, cancellationToken);
        if (project == null)
            return Result<Guid>.Failure($"Project {request.ProjectId} not found.");

        try
        {
            var milestone = new Milestone(
                request.ProjectId, request.Title, request.Description,
                request.Amount, request.Currency, request.Order, request.DueDate);

            project.AddMilestone(milestone);
            _projectRepository.Update(project);
            await _projectRepository.UnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Milestone {MilestoneId} added to project {ProjectId}", milestone.Id, request.ProjectId);
            return Result<Guid>.Success(milestone.Id);
        }
        catch (InvalidOperationException ex)
        {
            return Result<Guid>.Failure(ex.Message);
        }
    }
}
