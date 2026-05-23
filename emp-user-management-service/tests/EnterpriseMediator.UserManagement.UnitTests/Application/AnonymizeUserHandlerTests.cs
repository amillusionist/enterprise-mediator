using EnterpriseMediator.UserManagement.Application.Features.Users.Commands.AnonymizeUser;
using EnterpriseMediator.UserManagement.Application.Interfaces;
using EnterpriseMediator.UserManagement.Domain.Aggregates.User;
using EnterpriseMediator.UserManagement.Domain.Enums;
using EnterpriseMediator.UserManagement.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EnterpriseMediator.UserManagement.UnitTests.Application;

public class AnonymizeUserHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IAuditServiceAdapter> _auditMock = new();
    private readonly Mock<ILogger<AnonymizeUserHandler>> _loggerMock = new();
    private readonly AnonymizeUserHandler _handler;

    public AnonymizeUserHandlerTests()
    {
        _handler = new AnonymizeUserHandler(_userRepoMock.Object, _auditMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingUser_AnonymizesAndReturnsTrue()
    {
        var userId = Guid.NewGuid();
        var user = User.Create("test@example.com", "John", "Doe", "hash", UserType.Client);

        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new AnonymizeUserCommand(userId, "GDPR Request");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        user.FirstName.Should().Be("Anonymized");
        _userRepoMock.Verify(r => r.Update(user), Times.Once);
        _userRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _auditMock.Verify(a => a.LogEventAsync(It.Is<AuditLogEntry>(e => e.Action == "UserAnonymized"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsKeyNotFound()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new AnonymizeUserCommand(Guid.NewGuid(), "GDPR Request");
        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
