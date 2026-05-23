using EnterpriseMediator.Financial.Domain.Services;
using EnterpriseMediator.Financial.Domain.ValueObjects;
using FluentAssertions;

namespace EnterpriseMediator.Financial.UnitTests.Domain.Services;

public class InvoiceCalculationServiceTests
{
    private readonly InvoiceCalculationService _sut = new();

    [Fact]
    public void CalculateInvoiceBreakdown_WithStandardInputs_ShouldReturnCorrectBreakdown()
    {
        var projectAmount = new Money(10000m, Currency.USD);

        var result = _sut.CalculateInvoiceBreakdown(projectAmount, 10m, 20m);

        result.BaseAmount.Amount.Should().Be(10000m);
        result.PlatformFee.Amount.Should().Be(1000m);        // 10% of 10000
        result.TaxAmount.Amount.Should().Be(2200m);           // 20% of (10000 + 1000)
        result.TotalClientAmount.Amount.Should().Be(13200m);  // 10000 + 1000 + 2200
        result.MarginPercentageApplied.Should().Be(10m);
        result.TaxPercentageApplied.Should().Be(20m);
    }

    [Fact]
    public void CalculateInvoiceBreakdown_WithZeroMargin_ShouldHaveNoFee()
    {
        var projectAmount = new Money(5000m, Currency.USD);

        var result = _sut.CalculateInvoiceBreakdown(projectAmount, 0m, 10m);

        result.PlatformFee.Amount.Should().Be(0m);
        result.TaxAmount.Amount.Should().Be(500m);
        result.TotalClientAmount.Amount.Should().Be(5500m);
    }

    [Fact]
    public void CalculateInvoiceBreakdown_WithZeroTax_ShouldHaveNoTax()
    {
        var projectAmount = new Money(5000m, Currency.USD);

        var result = _sut.CalculateInvoiceBreakdown(projectAmount, 15m, 0m);

        result.PlatformFee.Amount.Should().Be(750m);
        result.TaxAmount.Amount.Should().Be(0m);
        result.TotalClientAmount.Amount.Should().Be(5750m);
    }

    [Fact]
    public void CalculateInvoiceBreakdown_WithNullAmount_ShouldThrow()
    {
        var act = () => _sut.CalculateInvoiceBreakdown(null!, 10m, 20m);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CalculateInvoiceBreakdown_WithNegativeMargin_ShouldThrow()
    {
        var amount = new Money(1000m, Currency.USD);
        var act = () => _sut.CalculateInvoiceBreakdown(amount, -5m, 20m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CalculateInvoiceBreakdown_WithNegativeTax_ShouldThrow()
    {
        var amount = new Money(1000m, Currency.USD);
        var act = () => _sut.CalculateInvoiceBreakdown(amount, 10m, -3m);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateCalculation_WithCorrectResult_ShouldReturnTrue()
    {
        var projectAmount = new Money(10000m, Currency.USD);
        var result = _sut.CalculateInvoiceBreakdown(projectAmount, 10m, 20m);

        _sut.ValidateCalculation(result).Should().BeTrue();
    }

    [Fact]
    public void ValidateCalculation_WithNull_ShouldReturnFalse()
    {
        _sut.ValidateCalculation(null!).Should().BeFalse();
    }

    [Fact]
    public void CalculateInvoiceBreakdown_ShouldMaintainCurrencyConsistency()
    {
        var projectAmount = new Money(8000m, Currency.EUR);
        var result = _sut.CalculateInvoiceBreakdown(projectAmount, 12m, 19m);

        result.BaseAmount.Currency.Should().Be(Currency.EUR);
        result.PlatformFee.Currency.Should().Be(Currency.EUR);
        result.TaxAmount.Currency.Should().Be(Currency.EUR);
        result.TotalClientAmount.Currency.Should().Be(Currency.EUR);
    }
}
