using Emp.ApiGateway.Application.Features.Users.Commands;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Emp.ApiGateway.UnitTests.Features.Users.Commands;

public class InviteUserValidatorTests
{
    private readonly InviteUserValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new InviteUserCommand("user@example.com", "VendorContact", Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_InvalidEmail_ShouldFail(string email)
    {
        var command = new InviteUserCommand(email, "VendorContact", Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Admin")]
    [InlineData("SuperUser")]
    public void Validate_InvalidRole_ShouldFail(string role)
    {
        var command = new InviteUserCommand("user@example.com", role, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Role);
    }

    [Theory]
    [InlineData("SystemAdministrator")]
    [InlineData("VendorContact")]
    [InlineData("ClientContact")]
    public void Validate_ValidRoles_ShouldPass(string role)
    {
        var command = new InviteUserCommand("user@example.com", role, Guid.NewGuid());

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(x => x.Role);
    }

    [Fact]
    public void Validate_EmptyInvitedBy_ShouldFail()
    {
        var command = new InviteUserCommand("user@example.com", "VendorContact", Guid.Empty);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.InvitedBy);
    }
}
