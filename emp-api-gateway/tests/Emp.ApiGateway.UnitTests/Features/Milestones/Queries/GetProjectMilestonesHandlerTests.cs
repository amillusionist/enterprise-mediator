using Emp.ApiGateway.Application.Features.Milestones.Queries;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Projects;
using EnterpriseMediator.Contracts.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Emp.ApiGateway.UnitTests.Features.Milestones.Queries;

public class GetProjectMilestonesHandlerTests
{
    private readonly IProjectServiceClient _projectService = Substitute.For<IProjectServiceClient>();
    private readonly ILogger<GetProjectMilestonesHandler> _logger = Substitute.For<ILogger<GetProjectMilestonesHandler>>();
    private readonly GetProjectMilestonesHandler _sut;

    public GetProjectMilestonesHandlerTests()
    {
        _sut = new GetProjectMilestonesHandler(_projectService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldReturnMilestones()
    {
        var projectId = Guid.NewGuid();
        var milestones = new List<MilestoneDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Title = "Phase 1",
                Amount = 25_000m,
                Currency = "USD",
                Status = MilestoneStatus.Pending
            },
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Title = "Phase 2",
                Amount = 35_000m,
                Currency = "USD",
                Status = MilestoneStatus.Pending
            }
        };

        _projectService
            .GetProjectMilestonesAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(milestones);

        var result = await _sut.Handle(new GetProjectMilestonesQuery(projectId), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].Title.Should().Be("Phase 1");
        result[1].Amount.Should().Be(35_000m);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoMilestones()
    {
        var projectId = Guid.NewGuid();

        _projectService
            .GetProjectMilestonesAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<MilestoneDto>());

        var result = await _sut.Handle(new GetProjectMilestonesQuery(projectId), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
