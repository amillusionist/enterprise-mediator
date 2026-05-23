using Emp.ApiGateway.Application.Features.Projects.Commands;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Emp.ApiGateway.UnitTests.Features.Projects.Commands;

public class CreateProjectCommandValidatorTests
{
    private readonly CreateProjectCommandValidator _validator = new();

    [Fact]
    public async Task Validate_ValidCommand_ShouldPass()
    {
        var command = new CreateProjectCommand
        {
            Name = "Valid Project",
            Description = "A valid project description",
            ClientId = Guid.NewGuid()
        };

        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_EmptyName_ShouldFail()
    {
        var command = new CreateProjectCommand
        {
            Name = "",
            ClientId = Guid.NewGuid()
        };

        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_NameTooLong_ShouldFail()
    {
        var command = new CreateProjectCommand
        {
            Name = new string('A', 101),
            ClientId = Guid.NewGuid()
        };

        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_EmptyClientId_ShouldFail()
    {
        var command = new CreateProjectCommand
        {
            Name = "Test",
            ClientId = Guid.Empty
        };

        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.ClientId);
    }

    [Fact]
    public async Task Validate_DescriptionTooLong_ShouldFail()
    {
        var command = new CreateProjectCommand
        {
            Name = "Test",
            ClientId = Guid.NewGuid(),
            Description = new string('A', 1001)
        };

        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Validate_EndDateBeforeStartDate_ShouldFail()
    {
        var command = new CreateProjectCommand
        {
            Name = "Test",
            ClientId = Guid.NewGuid(),
            StartDate = DateTime.UtcNow.AddMonths(1),
            EndDate = DateTime.UtcNow
        };

        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.EndDate);
    }

    [Fact]
    public async Task Validate_NoDates_ShouldPass()
    {
        var command = new CreateProjectCommand
        {
            Name = "Test",
            ClientId = Guid.NewGuid()
        };

        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.EndDate);
    }
}
