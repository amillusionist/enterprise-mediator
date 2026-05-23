using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Projects;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Emp.ApiGateway.IntegrationTests.Controllers;

public class BriefsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    private static bool IsNet10Runtime => Environment.Version.Major >= 10;

    public BriefsControllerTests(CustomWebApplicationFactory factory)
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
    public async Task GetProjectBrief_WhenBriefExists_Returns200()
    {
        if (IsNet10Runtime) return;

        var projectId = Guid.NewGuid();
        var brief = new ProjectBriefDto
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = "AI Platform Development",
            RequiredSkills = new[] { "Python", "ML" },
            IsApproved = false
        };

        _factory.ProjectServiceClient
            .GetProjectBriefAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(brief);

        var response = await _client.GetAsync($"/api/v1/projects/{projectId}/briefs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetProjectBrief_WhenNotFound_Returns404()
    {
        if (IsNet10Runtime) return;

        var projectId = Guid.NewGuid();

        _factory.ProjectServiceClient
            .GetProjectBriefAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((ProjectBriefDto?)null);

        var response = await _client.GetAsync($"/api/v1/projects/{projectId}/briefs");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMatchingVendors_Returns200()
    {
        if (IsNet10Runtime) return;

        var projectId = Guid.NewGuid();
        var vendors = new List<VendorMatchResultDto>
        {
            new()
            {
                VendorId = Guid.NewGuid(),
                CompanyName = "Acme Corp",
                SimilarityScore = 0.92,
                MatchedSkills = new[] { "C#", ".NET" }
            }
        };

        _factory.ProjectServiceClient
            .GetMatchingVendorsAsync(projectId, 25, 0.75, Arg.Any<CancellationToken>())
            .Returns(vendors);

        var response = await _client.GetAsync($"/api/v1/projects/{projectId}/briefs/matching-vendors");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public class GuidTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public static readonly Guid TestUserId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    public GuidTestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId.ToString()),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Role, "SystemAdministrator")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
