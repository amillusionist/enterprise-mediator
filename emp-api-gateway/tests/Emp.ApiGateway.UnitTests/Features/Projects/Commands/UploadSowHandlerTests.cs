using Emp.ApiGateway.Application.Features.Projects.Commands;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Emp.ApiGateway.UnitTests.Features.Projects.Commands;

public class UploadSowHandlerTests
{
    private readonly IProjectServiceClient _projectService = Substitute.For<IProjectServiceClient>();
    private readonly ILogger<UploadSowHandler> _logger = Substitute.For<ILogger<UploadSowHandler>>();
    private readonly UploadSowHandler _sut;

    public UploadSowHandlerTests()
    {
        _sut = new UploadSowHandler(_projectService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldForwardStreamToProjectService()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var stream = new MemoryStream("fake content"u8.ToArray());
        using var command = new UploadSowCommand(projectId, stream, "sow.pdf", "application/pdf");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(Unit.Value);

        await _projectService.Received(1).UploadSowAsync(
            projectId,
            stream,
            "sow.pdf",
            "application/pdf",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenServiceThrows_ShouldPropagateException()
    {
        var projectId = Guid.NewGuid();
        var stream = new MemoryStream();
        using var command = new UploadSowCommand(projectId, stream, "test.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

        _projectService
            .When(x => x.UploadSowAsync(Arg.Any<Guid>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()))
            .Throw(new HttpRequestException("Upload failed"));

        var act = () => _sut.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>();
    }
}
