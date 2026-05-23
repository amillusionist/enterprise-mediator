using Emp.ApiGateway.Application.Features.Briefs.Commands;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Projects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Emp.ApiGateway.UnitTests.Features.Briefs.Commands;

public class ApproveProjectBriefHandlerTests
{
    private readonly IProjectServiceClient _projectService = Substitute.For<IProjectServiceClient>();
    private readonly ILogger<ApproveProjectBriefHandler> _logger = Substitute.For<ILogger<ApproveProjectBriefHandler>>();
    private readonly ApproveProjectBriefHandler _sut;

    public ApproveProjectBriefHandlerTests()
    {
        _sut = new ApproveProjectBriefHandler(_projectService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldApproveAndReturnUpdatedBrief()
    {
        var projectId = Guid.NewGuid();
        var edits = new UpdateProjectBriefRequest
        {
            Title = "Updated Title",
            RequiredSkills = new[] { "React", "TypeScript" }
        };

        var expectedBrief = new ProjectBriefDto
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = "Updated Title",
            RequiredSkills = new[] { "React", "TypeScript" },
            IsApproved = true
        };

        _projectService
            .ApproveProjectBriefAsync(projectId, edits, Arg.Any<CancellationToken>())
            .Returns(expectedBrief);

        var result = await _sut.Handle(new ApproveProjectBriefCommand(projectId, edits), CancellationToken.None);

        result.Should().NotBeNull();
        result.IsApproved.Should().BeTrue();
        result.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task Handle_ShouldApproveWithoutEdits()
    {
        var projectId = Guid.NewGuid();

        var expectedBrief = new ProjectBriefDto
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = "Original",
            RequiredSkills = new[] { "C#" },
            IsApproved = true
        };

        _projectService
            .ApproveProjectBriefAsync(projectId, null, Arg.Any<CancellationToken>())
            .Returns(expectedBrief);

        var result = await _sut.Handle(new ApproveProjectBriefCommand(projectId, null), CancellationToken.None);

        result.IsApproved.Should().BeTrue();
        await _projectService.Received(1).ApproveProjectBriefAsync(projectId, null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenServiceThrows_ShouldPropagate()
    {
        var projectId = Guid.NewGuid();

        _projectService
            .ApproveProjectBriefAsync(projectId, Arg.Any<UpdateProjectBriefRequest?>(), Arg.Any<CancellationToken>())
            .Returns<ProjectBriefDto>(x => throw new HttpRequestException("Service unavailable"));

        var act = () => _sut.Handle(new ApproveProjectBriefCommand(projectId, null), CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>().WithMessage("Service unavailable");
    }
}
