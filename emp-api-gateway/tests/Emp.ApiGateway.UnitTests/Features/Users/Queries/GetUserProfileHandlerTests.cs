using Emp.ApiGateway.Application.Features.Users.Queries;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Users;
using EnterpriseMediator.Contracts.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Emp.ApiGateway.UnitTests.Features.Users.Queries;

public class GetUserProfileHandlerTests
{
    private readonly IUserServiceClient _userService = Substitute.For<IUserServiceClient>();
    private readonly ILogger<GetUserProfileHandler> _logger = Substitute.For<ILogger<GetUserProfileHandler>>();
    private readonly GetUserProfileHandler _sut;

    public GetUserProfileHandlerTests()
    {
        _sut = new GetUserProfileHandler(_userService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldReturnProfile_WhenUserExists()
    {
        var userId = Guid.NewGuid();
        var expected = new UserProfileDto
        {
            Id = userId,
            Email = "test@example.com",
            FullName = "Test User",
            Role = UserRole.SystemAdministrator,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            LastLoginAt = DateTimeOffset.UtcNow
        };

        _userService
            .GetUserProfileAsync(userId, Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _sut.Handle(new GetUserProfileQuery(userId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Email.Should().Be("test@example.com");
        result.FullName.Should().Be("Test User");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserNotFound()
    {
        var userId = Guid.NewGuid();

        _userService
            .GetUserProfileAsync(userId, Arg.Any<CancellationToken>())
            .Returns((UserProfileDto?)null);

        var result = await _sut.Handle(new GetUserProfileQuery(userId), CancellationToken.None);

        result.Should().BeNull();
    }
}
