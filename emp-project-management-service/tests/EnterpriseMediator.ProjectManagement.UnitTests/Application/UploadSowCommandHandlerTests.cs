using EnterpriseMediator.ProjectManagement.Application.Common;
using EnterpriseMediator.ProjectManagement.Application.Features.Projects.Commands.UploadSow;
using EnterpriseMediator.ProjectManagement.Application.Interfaces;
using EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;
using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EnterpriseMediator.ProjectManagement.UnitTests.Application;

public class UploadSowCommandHandlerTests
{
    private readonly Mock<IProjectRepository> _projectRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<UploadSowCommandHandler>> _loggerMock;
    private readonly UploadSowCommandHandler _sut;

    public UploadSowCommandHandlerTests()
    {
        _projectRepositoryMock = new Mock<IProjectRepository>();
        _fileStorageMock = new Mock<IFileStorageService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<UploadSowCommandHandler>>();

        _projectRepositoryMock.Setup(r => r.UnitOfWork).Returns(_unitOfWorkMock.Object);

        _sut = new UploadSowCommandHandler(
            _projectRepositoryMock.Object,
            _fileStorageMock.Object,
            _loggerMock.Object);
    }

    private static Project CreatePendingProject()
    {
        return Project.Create(
            clientId: Guid.NewGuid(),
            name: "Test Project",
            description: "A test project");
    }

    [Fact]
    public async Task Handle_ValidCommand_UploadsFileAndReturnsDocumentId()
    {
        // Arrange
        var project = CreatePendingProject();
        var projectId = project.Id;
        var storageKey = $"sow/{Guid.NewGuid()}/test-sow.pdf";
        using var fileStream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03 });

        var command = new UploadSowCommand(
            ProjectId: projectId,
            FileStream: fileStream,
            FileName: "test-sow.pdf",
            ContentType: "application/pdf");

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _fileStorageMock
            .Setup(f => f.UploadAsync(
                command.FileStream,
                command.FileName,
                command.ContentType,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(storageKey);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        _fileStorageMock.Verify(
            f => f.UploadAsync(
                command.FileStream,
                command.FileName,
                command.ContentType,
                It.IsAny<CancellationToken>()),
            Times.Once);

        project.SowDocument.Should().NotBeNull();
        project.SowDocument!.FileName.Should().Be("test-sow.pdf");
        project.SowDocument.StorageKey.Should().Be(storageKey);
    }

    [Fact]
    public async Task Handle_ProjectNotFound_ReturnsFailure()
    {
        // Arrange
        var nonExistentProjectId = Guid.NewGuid();
        using var fileStream = new MemoryStream(new byte[] { 0x01 });

        var command = new UploadSowCommand(
            ProjectId: nonExistentProjectId,
            FileStream: fileStream,
            FileName: "test-sow.pdf",
            ContentType: "application/pdf");

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(nonExistentProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Project?)null);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");

        _fileStorageMock.Verify(
            f => f.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommand_SavesChanges()
    {
        // Arrange
        var project = CreatePendingProject();
        var projectId = project.Id;
        using var fileStream = new MemoryStream(new byte[] { 0x01, 0x02, 0x03, 0x04 });

        var command = new UploadSowCommand(
            ProjectId: projectId,
            FileStream: fileStream,
            FileName: "statement-of-work.docx",
            ContentType: "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _fileStorageMock
            .Setup(f => f.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("sow/key/statement-of-work.docx");

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _projectRepositoryMock.Verify(
            r => r.Update(project),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidCommand_RaisesSowUploadedDomainEvent()
    {
        // Arrange
        var project = CreatePendingProject();
        project.ClearDomainEvents(); // Clear the ProjectCreatedDomainEvent from Create()
        var projectId = project.Id;
        using var fileStream = new MemoryStream(new byte[] { 0x01, 0x02 });

        var command = new UploadSowCommand(
            ProjectId: projectId,
            FileStream: fileStream,
            FileName: "sow.pdf",
            ContentType: "application/pdf");

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _fileStorageMock
            .Setup(f => f.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("sow/key/sow.pdf");

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        project.DomainEvents.Should().Contain(e =>
            e.GetType().Name == "SowUploadedDomainEvent");
        project.DomainEvents.Should().Contain(e =>
            e.GetType().Name == "ProjectStatusChangedDomainEvent");
    }

    [Fact]
    public async Task Handle_ValidCommand_PassesCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var project = CreatePendingProject();
        var projectId = project.Id;
        using var fileStream = new MemoryStream(new byte[] { 0x01 });

        var command = new UploadSowCommand(
            ProjectId: projectId,
            FileStream: fileStream,
            FileName: "sow.pdf",
            ContentType: "application/pdf");

        _projectRepositoryMock
            .Setup(r => r.GetByIdAsync(projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _fileStorageMock
            .Setup(f => f.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("key");

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.Handle(command, cts.Token);

        // Assert
        _projectRepositoryMock.Verify(
            r => r.GetByIdAsync(projectId, cts.Token),
            Times.Once);

        _fileStorageMock.Verify(
            f => f.UploadAsync(fileStream, "sow.pdf", "application/pdf", cts.Token),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(cts.Token),
            Times.Once);
    }
}
