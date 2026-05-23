using EnterpriseMediator.Financial.Application.Features.Invoices.Commands.GenerateInvoice;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace EnterpriseMediator.Financial.UnitTests.Application.Validators;

public class GenerateInvoiceValidatorTests
{
    private readonly GenerateInvoiceValidator _sut = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        var command = new GenerateInvoiceCommand(Guid.NewGuid(), Guid.NewGuid(), 5000m, "USD");
        var result = _sut.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyProjectId_ShouldFail()
    {
        var command = new GenerateInvoiceCommand(Guid.Empty, Guid.NewGuid(), 5000m, "USD");
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.ProjectId);
    }

    [Fact]
    public void Validate_WithEmptyClientId_ShouldFail()
    {
        var command = new GenerateInvoiceCommand(Guid.NewGuid(), Guid.Empty, 5000m, "USD");
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.ClientId);
    }

    [Fact]
    public void Validate_WithZeroAmount_ShouldFail()
    {
        var command = new GenerateInvoiceCommand(Guid.NewGuid(), Guid.NewGuid(), 0m, "USD");
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Amount);
    }

    [Fact]
    public void Validate_WithNegativeAmount_ShouldFail()
    {
        var command = new GenerateInvoiceCommand(Guid.NewGuid(), Guid.NewGuid(), -100m, "USD");
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.Amount);
    }

    [Fact]
    public void Validate_WithInvalidCurrencyCode_ShouldFail()
    {
        var command = new GenerateInvoiceCommand(Guid.NewGuid(), Guid.NewGuid(), 5000m, "US");
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CurrencyCode);
    }

    [Fact]
    public void Validate_WithEmptyCurrencyCode_ShouldFail()
    {
        var command = new GenerateInvoiceCommand(Guid.NewGuid(), Guid.NewGuid(), 5000m, "");
        var result = _sut.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.CurrencyCode);
    }
}
