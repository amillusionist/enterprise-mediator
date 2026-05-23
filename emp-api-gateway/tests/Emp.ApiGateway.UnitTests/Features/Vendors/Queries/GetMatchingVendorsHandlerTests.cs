using Emp.ApiGateway.Application.Features.Vendors.Queries;
using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Projects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Emp.ApiGateway.UnitTests.Features.Vendors.Queries;

public class GetMatchingVendorsHandlerTests
{
    private readonly IProjectServiceClient _projectService = Substitute.For<IProjectServiceClient>();
    private readonly ILogger<GetMatchingVendorsHandler> _logger = Substitute.For<ILogger<GetMatchingVendorsHandler>>();
    private readonly GetMatchingVendorsHandler _sut;

    public GetMatchingVendorsHandlerTests()
    {
        _sut = new GetMatchingVendorsHandler(_projectService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldReturnMatchingVendors()
    {
        var projectId = Guid.NewGuid();
        var vendors = new List<VendorMatchResultDto>
        {
            new()
            {
                VendorId = Guid.NewGuid(),
                CompanyName = "Acme Corp",
                SimilarityScore = 0.92,
                MatchedSkills = new[] { "C#", ".NET" }
            },
            new()
            {
                VendorId = Guid.NewGuid(),
                CompanyName = "Beta LLC",
                SimilarityScore = 0.85,
                MatchedSkills = new[] { "C#" }
            }
        };

        _projectService
            .GetMatchingVendorsAsync(projectId, 25, 0.75, Arg.Any<CancellationToken>())
            .Returns(vendors);

        var result = await _sut.Handle(new GetMatchingVendorsQuery(projectId), CancellationToken.None);

        result.Should().HaveCount(2);
        result[0].SimilarityScore.Should().BeGreaterThan(result[1].SimilarityScore);
    }

    [Fact]
    public async Task Handle_ShouldPassCustomParameters()
    {
        var projectId = Guid.NewGuid();

        _projectService
            .GetMatchingVendorsAsync(projectId, 10, 0.9, Arg.Any<CancellationToken>())
            .Returns(new List<VendorMatchResultDto>());

        var result = await _sut.Handle(new GetMatchingVendorsQuery(projectId, 10, 0.9), CancellationToken.None);

        result.Should().BeEmpty();
        await _projectService.Received(1).GetMatchingVendorsAsync(projectId, 10, 0.9, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoMatches()
    {
        var projectId = Guid.NewGuid();

        _projectService
            .GetMatchingVendorsAsync(projectId, 25, 0.75, Arg.Any<CancellationToken>())
            .Returns(new List<VendorMatchResultDto>());

        var result = await _sut.Handle(new GetMatchingVendorsQuery(projectId), CancellationToken.None);

        result.Should().BeEmpty();
    }
}
