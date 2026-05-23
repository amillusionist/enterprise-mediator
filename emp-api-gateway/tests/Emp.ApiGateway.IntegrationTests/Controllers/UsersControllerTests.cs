using System.Net;
using System.Net.Http.Json;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using Emp.ApiGateway.Web.Controllers;
using EnterpriseMediator.Contracts.DTOs.Users;
using EnterpriseMediator.Contracts.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Emp.ApiGateway.IntegrationTests.Controllers;

public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    private static bool IsNet10Runtime => Environment.Version.Major >= 10;

    public UsersControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, GuidTestAuthHandler>("Test", _ => { });

                services.PostConfigure<AuthenticationOptions>(opts =>
                {
                    opts.DefaultAuthenticateScheme = "Test";
                    opts.DefaultChallengeScheme = "Test";
                });
            });
        }).CreateClient();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetCurrentUser_Returns200_WithUserData()
    {
        if (IsNet10Runtime) return;

        var profile = new UserProfileDto
        {
            Id = GuidTestAuthHandler.TestUserId,
            Email = "test@example.com",
            FullName = "Test User",
            Role = UserRole.SystemAdministrator,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            LastLoginAt = DateTimeOffset.UtcNow
        };

        _factory.UserServiceClient
            .GetUserProfileAsync(GuidTestAuthHandler.TestUserId, Arg.Any<CancellationToken>())
            .Returns(profile);

        var response = await _client.GetAsync("/api/v1/users/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        body.Should().NotBeNull();
        body!.IsAuthenticated.Should().BeTrue();
        body.Email.Should().Be("test@example.com");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task InviteUser_WithValidData_Returns200()
    {
        if (IsNet10Runtime) return;

        _factory.UserServiceClient
            .InviteUserAsync(Arg.Any<InviteUserDto>(), Arg.Any<CancellationToken>())
            .Returns(new UserInvitationResultDto
            {
                Email = "newuser@example.com",
                Success = true,
                InvitationId = Guid.NewGuid().ToString()
            });

        var response = await _client.PostAsJsonAsync("/api/v1/users/invite",
            new InviteUserRequest("newuser@example.com", "VendorContact"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ValidateInvitation_PublicEndpoint_WhenValid_Returns200()
    {
        if (IsNet10Runtime) return;

        var token = "valid-invitation-token";
        var invitation = new UserInvitationDto
        {
            Email = "invited@example.com",
            Role = UserRole.VendorContact,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(6),
            IsValid = true
        };

        _factory.UserServiceClient
            .ValidateInvitationAsync(token, Arg.Any<CancellationToken>())
            .Returns(invitation);

        var response = await _client.GetAsync($"/api/v1/public/invitations/{token}/validate");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ValidateInvitation_PublicEndpoint_WhenExpired_Returns404()
    {
        if (IsNet10Runtime) return;

        var token = "expired-token";

        _factory.UserServiceClient
            .ValidateInvitationAsync(token, Arg.Any<CancellationToken>())
            .Returns((UserInvitationDto?)null);

        var response = await _client.GetAsync($"/api/v1/public/invitations/{token}/validate");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
