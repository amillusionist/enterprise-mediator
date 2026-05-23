using EnterpriseMediator.UserManagement.Domain.Aggregates.User;
using EnterpriseMediator.UserManagement.Domain.Enums;
using EnterpriseMediator.UserManagement.Domain.Events;
using FluentAssertions;

namespace EnterpriseMediator.UserManagement.UnitTests.Domain;

public class UserTests
{
    [Fact]
    public void Create_ValidInput_ReturnsUserWithCorrectProperties()
    {
        var user = User.Create("test@example.com", "John", "Doe", "hash123", UserType.Client);

        user.Email.Should().Be("test@example.com");
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.Type.Should().Be(UserType.Client);
        user.IsActive.Should().BeTrue();
        user.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_RaisesUserRegisteredEvent()
    {
        var user = User.Create("test@example.com", "John", "Doe", "hash123", UserType.Vendor);

        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserRegisteredEvent>();
    }

    [Fact]
    public void Create_NormalizesEmail()
    {
        var user = User.Create("  TEST@EXAMPLE.COM  ", "John", "Doe", "hash123", UserType.Internal);

        user.Email.Should().Be("test@example.com");
    }

    [Theory]
    [InlineData("", "John", "Doe", "hash")]
    [InlineData("test@example.com", "", "Doe", "hash")]
    [InlineData("test@example.com", "John", "", "hash")]
    [InlineData("test@example.com", "John", "Doe", "")]
    public void Create_InvalidInput_ThrowsArgumentException(string email, string firstName, string lastName, string passwordHash)
    {
        var act = () => User.Create(email, firstName, lastName, passwordHash, UserType.Client);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateProfile_ValidInput_UpdatesProperties()
    {
        var user = User.Create("test@example.com", "John", "Doe", "hash123", UserType.Client);

        user.UpdateProfile("Jane", "Smith");

        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
    }

    [Fact]
    public void Deactivate_ActiveUser_SetsIsActiveFalse()
    {
        var user = User.Create("test@example.com", "John", "Doe", "hash123", UserType.Client);

        user.Deactivate();

        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_InactiveUser_SetsIsActiveTrue()
    {
        var user = User.Create("test@example.com", "John", "Doe", "hash123", UserType.Client);
        user.Deactivate();

        user.Activate();

        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Anonymize_ScrubsPiiAndRaisesEvent()
    {
        var user = User.Create("test@example.com", "John", "Doe", "hash123", UserType.Client);
        user.ClearDomainEvents();

        user.Anonymize();

        user.FirstName.Should().Be("Anonymized");
        user.LastName.Should().Be("User");
        user.Email.Should().Contain("anonymized_");
        user.PasswordHash.Should().BeEmpty();
        user.IsActive.Should().BeFalse();
        user.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserAnonymizedEvent>();
    }

    [Fact]
    public void RecordLogin_SetsLastLoginAt()
    {
        var user = User.Create("test@example.com", "John", "Doe", "hash123", UserType.Client);

        user.RecordLogin();

        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }
}
