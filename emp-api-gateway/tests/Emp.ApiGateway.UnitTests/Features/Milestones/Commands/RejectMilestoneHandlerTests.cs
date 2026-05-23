using Emp.ApiGateway.Application.Features.Milestones.Commands;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Projects;
using EnterpriseMediator.Contracts.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Emp.ApiGateway.UnitTests.Features.Milestones.Commands;

public class RejectMilestoneHandlerTests
{
    private readonly IProjectServiceClient _projectService = Substitute.For<IProjectServiceClient>();
    private readonly ILogger<RejectMilestoneHandler> _logger = Substitute.For<ILogger<RejectMilestoneHandler>>();
    private readonly RejectMilestoneHandler _sut;

    public RejectMilestoneHandlerTests()
    {
        _sut = new RejectMilestoneHandler(_projectService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldRejectAndReturnMilestone()
    {
        var milestoneId = Guid.NewGuid();
        var reason = "Quality does not meet requirements";

        var expected = new MilestoneDto
        {
            Id = milestoneId,
            ProjectId = Guid.NewGuid(),
            Title = "Phase 1",
            Amount = 25_000m,
            Currency = "USD",
            Status = MilestoneStatus.Rejected,
            RejectionReason = reason
        };

        _projectService
            .RejectMilestoneAsync(milestoneId, reason, Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _sut.Handle(new RejectMilestoneCommand(milestoneId, reason), CancellationToken.None);

        result.Status.Should().Be(MilestoneStatus.Rejected);
        result.RejectionReason.Should().Be(reason);
    }

    [Fact]
    public async Task Handle_ShouldPassReasonToService()
    {
        var milestoneId = Guid.NewGuid();
        var reason = "Incomplete deliverables";

        _projectService
            .RejectMilestoneAsync(milestoneId, reason, Arg.Any<CancellationToken>())
            .Returns(new MilestoneDto
            {
                Id = milestoneId,
                ProjectId = Guid.NewGuid(),
                Title = "Phase 1",
                Amount = 10_000m,
                Currency = "USD",
                Status = MilestoneStatus.Rejected,
                RejectionReason = reason
            });

        await _sut.Handle(new RejectMilestoneCommand(milestoneId, reason), CancellationToken.None);

        await _projectService.Received(1).RejectMilestoneAsync(milestoneId, reason, Arg.Any<CancellationToken>());
    }
}
