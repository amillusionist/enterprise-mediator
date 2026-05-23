using AutoMapper;
using Emp.ApiGateway.Application.Features.Projects.Queries;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using Emp.ApiGateway.Application.Mappings;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Emp.ApiGateway.UnitTests.Features.Projects.Queries;

public class GetProjectDashboardHandlerTests
{
    private readonly IProjectServiceClient _projectService = Substitute.For<IProjectServiceClient>();
    private readonly IFinancialServiceClient _financialService = Substitute.For<IFinancialServiceClient>();
    private readonly IMapper _mapper;
    private readonly ILogger<GetProjectDashboardHandler> _logger = Substitute.For<ILogger<GetProjectDashboardHandler>>();
    private readonly GetProjectDashboardHandler _sut;

    public GetProjectDashboardHandlerTests()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<PublicApiMappingProfile>());
        _mapper = config.CreateMapper();
        _sut = new GetProjectDashboardHandler(_projectService, _financialService, _mapper, _logger);
    }

    [Fact]
    public async Task Handle_ShouldFetchBothServicesInParallel_AndMapResults()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var query = new GetProjectDashboardQuery(projectId);

        var internalProject = new InternalProjectDto
        {
            Id = projectId,
            Name = "Test Project",
            Description = "Desc",
            ClientId = Guid.NewGuid(),
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        var financials = new FinancialSummaryResponse
        {
            ProjectId = projectId,
            TotalBudget = 100_000m,
            TotalInvoiced = 50_000m,
            TotalPaid = 40_000m,
            PendingPayouts = 10_000m,
            Currency = "USD",
            HasOverdueInvoices = false
        };

        _projectService.GetProjectDetailsAsync(projectId, Arg.Any<CancellationToken>()).Returns(internalProject);
        _financialService.GetProjectFinancialSummaryAsync(projectId, Arg.Any<CancellationToken>()).Returns(financials);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Project.Should().NotBeNull();
        result.Project!.Id.Should().Be(projectId);
        result.Project.Name.Should().Be("Test Project");
        result.Project.Status.Should().Be("Active");

        result.Financials.Should().NotBeNull();
        result.Financials!.TotalBudget.Should().Be(100_000m);
        result.Financials.TotalSpent.Should().Be(40_000m); // Mapped from TotalPaid
        result.Financials.FinancialHealthStatus.Should().Be("Healthy");
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrowKeyNotFoundException()
    {
        var projectId = Guid.NewGuid();
        var query = new GetProjectDashboardQuery(projectId);

        _projectService.GetProjectDetailsAsync(projectId, Arg.Any<CancellationToken>()).Returns((InternalProjectDto?)null);
        _financialService.GetProjectFinancialSummaryAsync(projectId, Arg.Any<CancellationToken>()).Returns((FinancialSummaryResponse?)null);

        var act = () => _sut.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenFinancialsNull_ShouldReturnNullFinancials()
    {
        var projectId = Guid.NewGuid();
        var query = new GetProjectDashboardQuery(projectId);

        var internalProject = new InternalProjectDto
        {
            Id = projectId,
            Name = "Test",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _projectService.GetProjectDetailsAsync(projectId, Arg.Any<CancellationToken>()).Returns(internalProject);
        _financialService.GetProjectFinancialSummaryAsync(projectId, Arg.Any<CancellationToken>()).Returns((FinancialSummaryResponse?)null);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Project.Should().NotBeNull();
        result.Financials.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenOverdueInvoices_ShouldMapFinancialHealthToAtRisk()
    {
        var projectId = Guid.NewGuid();
        var query = new GetProjectDashboardQuery(projectId);

        var internalProject = new InternalProjectDto
        {
            Id = projectId,
            Name = "Risky",
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        var financials = new FinancialSummaryResponse
        {
            ProjectId = projectId,
            TotalBudget = 50_000m,
            TotalPaid = 20_000m,
            HasOverdueInvoices = true
        };

        _projectService.GetProjectDetailsAsync(projectId, Arg.Any<CancellationToken>()).Returns(internalProject);
        _financialService.GetProjectFinancialSummaryAsync(projectId, Arg.Any<CancellationToken>()).Returns(financials);

        var result = await _sut.Handle(query, CancellationToken.None);

        result.Financials!.FinancialHealthStatus.Should().Be("AtRisk");
    }
}
