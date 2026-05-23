using Emp.ApiGateway.Application.Features.Briefs.Queries;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Projects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Emp.ApiGateway.UnitTests.Features.Briefs.Queries;

public class GetProjectBriefHandlerTests
{
    private readonly IProjectServiceClient _projectService = Substitute.For<IProjectServiceClient>();
    private readonly ILogger<GetProjectBriefHandler> _logger = Substitute.For<ILogger<GetProjectBriefHandler>>();
    private readonly GetProjectBriefHandler _sut;

    public GetProjectBriefHandlerTests()
    {
        _sut = new GetProjectBriefHandler(_projectService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldReturnBrief_WhenBriefExists()
    {
        var projectId = Guid.NewGuid();
        var expectedBrief = new ProjectBriefDto
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = "Test Brief",
            RequiredSkills = new[] { "C#", ".NET" },
            IsApproved = false
        };

        _projectService
            .GetProjectBriefAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(expectedBrief);

        var result = await _sut.Handle(new GetProjectBriefQuery(projectId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ProjectId.Should().Be(projectId);
        result.Title.Should().Be("Test Brief");
        result.RequiredSkills.Should().Contain("C#");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenBriefNotFound()
    {
        var projectId = Guid.NewGuid();

        _projectService
            .GetProjectBriefAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((ProjectBriefDto?)null);

        var result = await _sut.Handle(new GetProjectBriefQuery(projectId), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldForwardCancellationToken()
    {
        var projectId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _projectService
            .GetProjectBriefAsync(projectId, token)
            .Returns((ProjectBriefDto?)null);

        await _sut.Handle(new GetProjectBriefQuery(projectId), token);

        await _projectService.Received(1).GetProjectBriefAsync(projectId, token);
    }
}
