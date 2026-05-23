using Emp.ApiGateway.Application.Features.Financials.Commands;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Emp.ApiGateway.UnitTests.Features.Financials.Commands;

public class GenerateInvoiceValidatorTests
{
    private readonly GenerateInvoiceValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new GenerateInvoiceCommand
        {
            ProjectId = Guid.NewGuid(),
            Amount = 10_000m,
            Currency = "USD",
            Description = "Test invoice"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyProjectId_ShouldFail()
    {
        var command = new GenerateInvoiceCommand
        {
            ProjectId = Guid.Empty,
            Amount = 10_000m,
            Currency = "USD"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ProjectId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Validate_InvalidAmount_ShouldFail(decimal amount)
    {
        var command = new GenerateInvoiceCommand
        {
            ProjectId = Guid.NewGuid(),
            Amount = amount,
            Currency = "USD"
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("US")]
    [InlineData("USDD")]
    public void Validate_InvalidCurrency_ShouldFail(string currency)
    {
        var command = new GenerateInvoiceCommand
        {
            ProjectId = Guid.NewGuid(),
            Amount = 1_000m,
            Currency = currency
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void Validate_DescriptionTooLong_ShouldFail()
    {
        var command = new GenerateInvoiceCommand
        {
            ProjectId = Guid.NewGuid(),
            Amount = 1_000m,
            Currency = "USD",
            Description = new string('x', 501)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}
