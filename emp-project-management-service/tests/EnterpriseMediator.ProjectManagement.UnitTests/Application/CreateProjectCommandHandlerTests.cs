using EnterpriseMediator.ProjectManagement.Application.Common;
using EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.CreateProject;
using EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;
using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EnterpriseMediator.ProjectManagement.UnitTests.Application;

public class CreateProjectCommandHandlerTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateProjectCommandHandler>> _loggerMock;
    private readonly CreateProjectCommandHandler _sut;

    public CreateProjectCommandHandlerTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreateProjectCommandHandler>>();

        _projectRepositoryMock.Setup(r => r.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _sut = new CreateProjectCommandHandler(
            _projectRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesProjectAndReturnsId()
    {
        // Arrange
        var command = new CreateProjectCommand(
            ClientId: Guid.NewGuid(),
            Name: "Test Project",
            Description: "A test project description");

        _projectRepositoryMock
            .Setup(r => r.ExistsByNameAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        result.Error.Should().BeNull();

        _projectRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Project>(p =>
                p.Name == command.Name &&
                p.ClientId == command.ClientId &&
                p.Description == command.Description),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var command = new CreateProjectCommand(
            ClientId: Guid.NewGuid(),
            Name: "Another Project",
            Description: "Description");

        _projectRepositoryMock
            .Setup(r => r.ExistsByNameAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_PublishesDomainEvents()
    {
        // Arrange
        var command = new CreateProjectCommand(
            ClientId: Guid.NewGuid(),
            Name: "Project With Events",
            Description: "Description");

        Project? capturedProject = null;

        _projectRepositoryMock
            .Setup(r => r.ExistsByNameAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _projectRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()))
            .Callback<Project, CancellationToken>((p, _) => capturedProject = p)
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        capturedProject.Should().NotBeNull();
        capturedProject!.DomainEvents.Should().NotBeEmpty();
        capturedProject.DomainEvents.Should().ContainSingle(e =>
            e.GetType().Name == "ProjectCreatedDomainEvent");
    }

    [Fact]
    public async Task Handle_DuplicateName_ReturnsFailure()
    {
        // Arrange
        var command = new CreateProjectCommand(
            ClientId: Guid.NewGuid(),
            Name: "Existing Project",
            Description: "Description");

        _projectRepositoryMock
            .Setup(r => r.ExistsByNameAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");

        _projectRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var command = new CreateProjectCommand(
            ClientId: Guid.NewGuid(),
            Name: "Cancellable Project",
            Description: "Description");

        _projectRepositoryMock
            .Setup(r => r.ExistsByNameAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.Handle(command, cts.Token);

        // Assert
        _projectRepositoryMock.Verify(
            r => r.ExistsByNameAsync(command.Name, null, cts.Token),
            Times.Once);

        _projectRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Project>(), cts.Token),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(cts.Token),
            Times.Once);
    }
}
