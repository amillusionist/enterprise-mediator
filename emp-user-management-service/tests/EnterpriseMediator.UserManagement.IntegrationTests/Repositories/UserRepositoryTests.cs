using EnterpriseMediator.UserManagement.Domain.Aggregates.User;
using EnterpriseMediator.UserManagement.Domain.Enums;
using EnterpriseMediator.UserManagement.Infrastructure.Persistence.Repositories;
using EnterpriseMediator.UserManagement.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace EnterpriseMediator.UserManagement.IntegrationTests.Repositories;

public class UserRepositoryTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;
    private readonly UserRepository _repository;

    public UserRepositoryTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _repository = new UserRepository(
            fixture.DbContext,
            new Mock<ILogger<UserRepository>>().Object);
    }

    [Fact]
    public async Task AddAndRetrieveUser_ById_ReturnsUser()
    {
        var user = User.Create("repo-test@example.com", "John", "Doe", "hash123", UserType.Client);

        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        var retrieved = await _repository.GetByIdAsync(user.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Email.Should().Be("repo-test@example.com");
        retrieved.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetByEmail_ExistingEmail_ReturnsUser()
    {
        var user = User.Create("byemail@example.com", "Jane", "Smith", "hash456", UserType.Vendor);

        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        var retrieved = await _repository.GetByEmailAsync("byemail@example.com");

        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task ExistsByEmail_ExistingEmail_ReturnsTrue()
    {
        var user = User.Create("exists@example.com", "Jane", "Smith", "hash789", UserType.Internal);

        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        var exists = await _repository.ExistsByEmailAsync("exists@example.com");

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmail_NonExistent_ReturnsFalse()
    {
        var exists = await _repository.ExistsByEmailAsync("nonexistent@example.com");

        exists.Should().BeFalse();
    }
}
