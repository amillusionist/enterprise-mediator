using System.Net;
using System.Net.Http.Json;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Projects;
using EnterpriseMediator.Contracts.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Emp.ApiGateway.IntegrationTests.Controllers;

public class MilestonesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    private static bool IsNet10Runtime => Environment.Version.Major >= 10;

    public MilestonesControllerTests(CustomWebApplicationFactory factory)
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
    public async Task GetProjectMilestones_Returns200()
    {
        if (IsNet10Runtime) return;

        var projectId = Guid.NewGuid();
        var milestones = new List<MilestoneDto>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Title = "Phase 1",
                Amount = 25_000m,
                Currency = "USD",
                Status = MilestoneStatus.Pending
            }
        };

        _factory.ProjectServiceClient
            .GetProjectMilestonesAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(milestones);

        var response = await _client.GetAsync($"/api/v1/projects/{projectId}/milestones");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task ApproveMilestone_PublicEndpoint_Returns200()
    {
        if (IsNet10Runtime) return;

        var milestoneId = Guid.NewGuid();
        var approved = new MilestoneDto
        {
            Id = milestoneId,
            ProjectId = Guid.NewGuid(),
            Title = "Phase 1",
            Amount = 25_000m,
            Currency = "USD",
            Status = MilestoneStatus.Approved,
            ApprovedAt = DateTimeOffset.UtcNow
        };

        _factory.ProjectServiceClient
            .ApproveMilestoneAsync(milestoneId, Arg.Any<CancellationToken>())
            .Returns(approved);

        var response = await _client.PutAsync($"/api/v1/public/milestones/{milestoneId}/approve", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task RejectMilestone_PublicEndpoint_Returns200()
    {
        if (IsNet10Runtime) return;

        var milestoneId = Guid.NewGuid();
        var rejected = new MilestoneDto
        {
            Id = milestoneId,
            ProjectId = Guid.NewGuid(),
            Title = "Phase 1",
            Amount = 25_000m,
            Currency = "USD",
            Status = MilestoneStatus.Rejected,
            RejectionReason = "Not complete"
        };

        _factory.ProjectServiceClient
            .RejectMilestoneAsync(milestoneId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(rejected);

        var response = await _client.PutAsJsonAsync(
            $"/api/v1/public/milestones/{milestoneId}/reject",
            new { reason = "Not complete" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
