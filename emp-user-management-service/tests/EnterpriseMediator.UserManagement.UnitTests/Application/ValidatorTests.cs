using EnterpriseMediator.UserManagement.Application.Features.Clients.Commands.CreateClient;
using EnterpriseMediator.UserManagement.Application.Features.Users.Commands.AnonymizeUser;
using EnterpriseMediator.UserManagement.Application.Features.Users.Commands.RegisterUser;
using EnterpriseMediator.UserManagement.Application.Features.Vendors.Commands.CreateVendor;
using FluentAssertions;

namespace EnterpriseMediator.UserManagement.UnitTests.Application;

public class ValidatorTests
{
    [Fact]
    public async Task CreateClientValidator_EmptyCompanyName_Fails()
    {
        var validator = new CreateClientValidator();
        var command = new CreateClientCommand { CompanyName = "", PrimaryContactEmail = "x@y.com", PrimaryContactFirstName = "A", PrimaryContactLastName = "B", AddressLine1 = "St", City = "C", Country = "US" };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CompanyName");
    }

    [Fact]
    public async Task CreateClientValidator_InvalidEmail_Fails()
    {
        var validator = new CreateClientValidator();
        var command = new CreateClientCommand { CompanyName = "Corp", PrimaryContactEmail = "not-an-email", PrimaryContactFirstName = "A", PrimaryContactLastName = "B", AddressLine1 = "St", City = "C", Country = "US" };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PrimaryContactEmail");
    }

    [Fact]
    public async Task CreateClientValidator_ValidCommand_Passes()
    {
        var validator = new CreateClientValidator();
        var command = new CreateClientCommand { CompanyName = "Corp", PrimaryContactEmail = "a@b.com", PrimaryContactFirstName = "A", PrimaryContactLastName = "B", AddressLine1 = "123 St", City = "City", Country = "US" };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateVendorValidator_ValidCommand_Passes()
    {
        var validator = new CreateVendorValidator();
        var command = new CreateVendorCommand { CompanyName = "Acme", PrimaryContactEmail = "v@a.com", PrimaryContactFirstName = "J", PrimaryContactLastName = "D", AddressLine1 = "St", City = "C", Country = "US", Skills = new() { "C#" } };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task AnonymizeUserValidator_EmptyReason_Fails()
    {
        var validator = new AnonymizeUserValidator();
        var command = new AnonymizeUserCommand(Guid.NewGuid(), "");

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterUserValidator_InvalidUserType_Fails()
    {
        var validator = new RegisterUserValidator();
        var command = new RegisterUserCommand { Email = "a@b.com", FirstName = "J", LastName = "D", Password = "12345678", UserType = "Invalid" };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserType");
    }

    [Fact]
    public async Task RegisterUserValidator_ValidCommand_Passes()
    {
        var validator = new RegisterUserValidator();
        var command = new RegisterUserCommand { Email = "a@b.com", FirstName = "J", LastName = "D", Password = "12345678", UserType = "Client" };

        var result = await validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }
}
