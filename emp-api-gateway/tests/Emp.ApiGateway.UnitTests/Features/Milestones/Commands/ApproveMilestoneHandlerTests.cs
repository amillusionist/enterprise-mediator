using Emp.ApiGateway.Application.Features.Milestones.Commands;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Projects;
using EnterpriseMediator.Contracts.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Emp.ApiGateway.UnitTests.Features.Milestones.Commands;

public class ApproveMilestoneHandlerTests
{
    private readonly IProjectServiceClient _projectService = Substitute.For<IProjectServiceClient>();
    private readonly ILogger<ApproveMilestoneHandler> _logger = Substitute.For<ILogger<ApproveMilestoneHandler>>();
    private readonly ApproveMilestoneHandler _sut;

    public ApproveMilestoneHandlerTests()
    {
        _sut = new ApproveMilestoneHandler(_projectService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldApproveAndReturnMilestone()
    {
        var milestoneId = Guid.NewGuid();
        var expected = new MilestoneDto
        {
            Id = milestoneId,
            ProjectId = Guid.NewGuid(),
            Title = "Phase 1",
            Amount = 25_000m,
            Currency = "USD",
            Status = MilestoneStatus.Approved,
            ApprovedAt = DateTimeOffset.UtcNow
        };

        _projectService
            .ApproveMilestoneAsync(milestoneId, Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _sut.Handle(new ApproveMilestoneCommand(milestoneId), CancellationToken.None);

        result.Status.Should().Be(MilestoneStatus.Approved);
        result.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenServiceThrows_ShouldPropagate()
    {
        var milestoneId = Guid.NewGuid();

        _projectService
            .ApproveMilestoneAsync(milestoneId, Arg.Any<CancellationToken>())
            .Returns<MilestoneDto>(x => throw new HttpRequestException("Not found"));

        var act = () => _sut.Handle(new ApproveMilestoneCommand(milestoneId), CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
