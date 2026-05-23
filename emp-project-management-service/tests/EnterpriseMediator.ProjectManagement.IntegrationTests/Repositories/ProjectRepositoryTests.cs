using EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;
using EnterpriseMediator.ProjectManagement.Domain.Enums;
using EnterpriseMediator.ProjectManagement.Infrastructure.Persistence;
using EnterpriseMediator.ProjectManagement.Infrastructure.Persistence.Repositories;
using EnterpriseMediator.ProjectManagement.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace EnterpriseMediator.ProjectManagement.IntegrationTests.Repositories;

[Collection("Postgres")]
public class ProjectRepositoryTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public ProjectRepositoryTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    private static ProjectRepository CreateRepository(ProjectDbContext context)
    {
        return new ProjectRepository(context, NullLogger<ProjectRepository>.Instance);
    }

    [Fact]
    public async Task AddAsync_And_GetByIdAsync_RoundTrips()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repo = CreateRepository(context);
        var project = Project.Create(Guid.NewGuid(), "Integration Test Project", "Testing persistence");

        // Act
        await repo.AddAsync(project);
        await repo.UnitOfWork.SaveChangesAsync();

        await using var readContext = _fixture.CreateDbContext();
        var readRepo = CreateRepository(readContext);
        var loaded = await readRepo.GetByIdAsync(project.Id);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be(project.Id);
        loaded.Name.Should().Be("Integration Test Project");
        loaded.Status.Should().Be(ProjectStatus.Pending);
    }

    [Fact]
    public async Task GetByClientIdAsync_ReturnsProjectsForClient()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repo = CreateRepository(context);
        var clientId = Guid.NewGuid();
        var project1 = Project.Create(clientId, "Client Project 1", "First");
        var project2 = Project.Create(clientId, "Client Project 2", "Second");
        var otherProject = Project.Create(Guid.NewGuid(), "Other Client Project", "Different client");

        await repo.AddAsync(project1);
        await repo.AddAsync(project2);
        await repo.AddAsync(otherProject);
        await repo.UnitOfWork.SaveChangesAsync();

        // Act
        await using var readContext = _fixture.CreateDbContext();
        var readRepo = CreateRepository(readContext);
        var results = await readRepo.GetByClientIdAsync(clientId);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(p => p.ClientId.Should().Be(clientId));
    }

    [Fact]
    public async Task ExistsByNameAsync_ReturnsTrueForExistingName()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repo = CreateRepository(context);
        var project = Project.Create(Guid.NewGuid(), $"Unique Name {Guid.NewGuid()}", "Test");

        await repo.AddAsync(project);
        await repo.UnitOfWork.SaveChangesAsync();

        // Act
        await using var readContext = _fixture.CreateDbContext();
        var readRepo = CreateRepository(readContext);
        var exists = await readRepo.ExistsByNameAsync(project.Name);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdWithProposalsAsync_IncludesProposals()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repo = CreateRepository(context);
        var project = Project.Create(Guid.NewGuid(), $"Proposal Test {Guid.NewGuid()}", "Test");

        // Advance to Proposed state to add proposals
        var sowDetails = new SowDetails(
            "Test scope", new[] { "Deliverable 1" }, new[] { "C#" },
            new[] { ".NET" }, "3 months");
        var sow = project.UploadSow("test.pdf", "application/pdf", 1024, "key/test.pdf", Guid.NewGuid());
        project.AttachSowDetails(sowDetails);
        project.ApproveBrief();
        project.DistributeBrief();

        var proposal = new Proposal(project.Id, Guid.NewGuid(), 50000m, "USD", "3 months", "John Doe", "Cover letter", null);
        project.AddProposal(proposal);

        await repo.AddAsync(project);
        await repo.UnitOfWork.SaveChangesAsync();

        // Act
        await using var readContext = _fixture.CreateDbContext();
        var readRepo = CreateRepository(readContext);
        var loaded = await readRepo.GetByIdWithProposalsAsync(project.Id);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.Proposals.Should().HaveCount(1);
        loaded.Proposals.First().VendorId.Should().Be(proposal.VendorId);
    }

    [Fact]
    public async Task Update_PersistsStatusChanges()
    {
        // Arrange
        await using var context = _fixture.CreateDbContext();
        var repo = CreateRepository(context);
        var project = Project.Create(Guid.NewGuid(), $"Status Test {Guid.NewGuid()}", "Test");

        await repo.AddAsync(project);
        await repo.UnitOfWork.SaveChangesAsync();

        // Act - upload SOW to change status
        project.UploadSow("test.pdf", "application/pdf", 1024, "key/test.pdf", Guid.NewGuid());
        repo.Update(project);
        await repo.UnitOfWork.SaveChangesAsync();

        // Assert
        await using var readContext = _fixture.CreateDbContext();
        var readRepo = CreateRepository(readContext);
        var loaded = await readRepo.GetByIdAsync(project.Id);
        loaded!.Status.Should().Be(ProjectStatus.Processing);
    }
}
