using Emp.ApiGateway.Application.Features.Projects.Commands;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Emp.ApiGateway.UnitTests.Features.Projects.Commands;

public class CreateProjectHandlerTests
{
    private readonly IProjectServiceClient _projectService = Substitute.For<IProjectServiceClient>();
    private readonly ILogger<CreateProjectHandler> _logger = Substitute.For<ILogger<CreateProjectHandler>>();
    private readonly CreateProjectHandler _sut;

    public CreateProjectHandlerTests()
    {
        _sut = new CreateProjectHandler(_projectService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldForwardToProjectService_AndReturnProjectId()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var command = new CreateProjectCommand
        {
            Name = "Test Project",
            Description = "A test project",
            ClientId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        _projectService
            .CreateProjectAsync(Arg.Any<CreateProjectDto>(), Arg.Any<CancellationToken>())
            .Returns(expectedId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedId);

        await _projectService.Received(1).CreateProjectAsync(
            Arg.Is<CreateProjectDto>(dto =>
                dto.Name == command.Name &&
                dto.Description == command.Description &&
                dto.ClientId == command.ClientId &&
                dto.StartDate == command.StartDate &&
                dto.EndDate == command.EndDate),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenServiceThrows_ShouldPropagateException()
    {
        var command = new CreateProjectCommand { Name = "Fail", ClientId = Guid.NewGuid() };

        _projectService
            .CreateProjectAsync(Arg.Any<CreateProjectDto>(), Arg.Any<CancellationToken>())
            .Returns<Guid>(x => throw new HttpRequestException("Service unavailable"));

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("Service unavailable");
    }
}
