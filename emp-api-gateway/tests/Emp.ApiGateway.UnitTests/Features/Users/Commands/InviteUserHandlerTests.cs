using Emp.ApiGateway.Application.Features.Users.Commands;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Emp.ApiGateway.UnitTests.Features.Users.Commands;

public class InviteUserHandlerTests
{
    private readonly IUserServiceClient _userService = Substitute.For<IUserServiceClient>();
    private readonly ILogger<InviteUserHandler> _logger = Substitute.For<ILogger<InviteUserHandler>>();
    private readonly InviteUserHandler _sut;

    public InviteUserHandlerTests()
    {
        _sut = new InviteUserHandler(_userService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldInviteAndReturnResult()
    {
        var invitedBy = Guid.NewGuid();
        var command = new InviteUserCommand("new@example.com", "VendorContact", invitedBy);

        var expected = new UserInvitationResultDto
        {
            Email = "new@example.com",
            Success = true,
            InvitationId = Guid.NewGuid().ToString()
        };

        _userService
            .InviteUserAsync(Arg.Any<InviteUserDto>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Success.Should().BeTrue();
        result.Email.Should().Be("new@example.com");
    }

    [Fact]
    public async Task Handle_ShouldMapCommandToDto()
    {
        var invitedBy = Guid.NewGuid();
        var command = new InviteUserCommand("vendor@test.com", "ClientContact", invitedBy);

        _userService
            .InviteUserAsync(Arg.Any<InviteUserDto>(), Arg.Any<CancellationToken>())
            .Returns(new UserInvitationResultDto
            {
                Email = "vendor@test.com",
                Success = true
            });

        await _sut.Handle(command, CancellationToken.None);

        await _userService.Received(1).InviteUserAsync(
            Arg.Is<InviteUserDto>(dto =>
                dto.Email == "vendor@test.com" &&
                dto.Role == "ClientContact" &&
                dto.InvitedBy == invitedBy),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenServiceFails_ShouldReturnFailureResult()
    {
        var command = new InviteUserCommand("existing@example.com", "VendorContact", Guid.NewGuid());

        _userService
            .InviteUserAsync(Arg.Any<InviteUserDto>(), Arg.Any<CancellationToken>())
            .Returns(new UserInvitationResultDto
            {
                Email = "existing@example.com",
                Success = false,
                ErrorMessage = "User already exists"
            });

        var result = await _sut.Handle(command, CancellationToken.None);

        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("User already exists");
    }
}
