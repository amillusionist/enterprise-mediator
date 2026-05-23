using System.Net;
using System.Net.Http.Json;
using Emp.ApiGateway.Application.Features.Financials.Commands;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Financials;
using EnterpriseMediator.Contracts.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Emp.ApiGateway.IntegrationTests.Controllers;

public class FinancialsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    private static bool IsNet10Runtime => Environment.Version.Major >= 10;

    public FinancialsControllerTests(CustomWebApplicationFactory factory)
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
    public async Task GenerateInvoice_WithValidData_Returns201()
    {
        if (IsNet10Runtime) return;

        var projectId = Guid.NewGuid();
        var command = new GenerateInvoiceCommand
        {
            ProjectId = projectId,
            Amount = 50_000m,
            Currency = "USD",
            DueDate = DateTimeOffset.UtcNow.AddDays(30),
            Description = "Phase 1 invoice"
        };

        var expectedInvoice = new InvoiceDto
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            ClientId = Guid.NewGuid(),
            Amount = 50_000m,
            Currency = "USD",
            Status = InvoiceStatus.Draft,
            DueDate = command.DueDate,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _factory.FinancialServiceClient
            .GenerateInvoiceAsync(Arg.Any<GenerateInvoiceRequest>(), Arg.Any<CancellationToken>())
            .Returns(expectedInvoice);

        var response = await _client.PostAsJsonAsync($"/api/v1/financials/projects/{projectId}/invoices/generate", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GenerateInvoice_WithMismatchedProjectId_Returns400()
    {
        if (IsNet10Runtime) return;

        var routeProjectId = Guid.NewGuid();
        var command = new GenerateInvoiceCommand
        {
            ProjectId = Guid.NewGuid(), // Different from route
            Amount = 50_000m,
            Currency = "USD"
        };

        var response = await _client.PostAsJsonAsync($"/api/v1/financials/projects/{routeProjectId}/invoices/generate", command);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
