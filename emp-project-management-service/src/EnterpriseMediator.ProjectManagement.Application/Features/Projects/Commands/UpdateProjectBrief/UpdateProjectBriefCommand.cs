using MediatR;

namespace EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.UpdateProjectBrief;

public record UpdateProjectBriefCommand(
    Guid ProjectId,
    string ScopeSummary,
    List<string> Deliverables,
    List<string> RequiredSkills,
    string Timeline,
    List<string> Technologies) : IRequest;
