using EnterpriseMediator.UserManagement.Application.Features.Internal.Queries.GetUserRole;
using EnterpriseMediator.UserManagement.Domain.Aggregates.User;
using EnterpriseMediator.UserManagement.Domain.Enums;
using EnterpriseMediator.UserManagement.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EnterpriseMediator.UserManagement.UnitTests.Application;

public class GetUserRoleHandlerTests
{
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<ILogger<GetUserRoleHandler>> _loggerMock = new();
    private readonly GetUserRoleHandler _handler;

    public GetUserRoleHandlerTests()
    {
        _handler = new GetUserRoleHandler(_userRepoMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData(UserType.Internal, "Internal")]
    [InlineData(UserType.Client, "Client")]
    [InlineData(UserType.Vendor, "Vendor")]
    public async Task Handle_ExistingUser_ReturnsCorrectRole(UserType type, string expectedRole)
    {
        var user = User.Create("test@example.com", "John", "Doe", "hash", type);

        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var query = new GetUserRoleQuery(user.Id);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().Be(expectedRole);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsKeyNotFound()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var query = new GetUserRoleQuery(Guid.NewGuid());
        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
