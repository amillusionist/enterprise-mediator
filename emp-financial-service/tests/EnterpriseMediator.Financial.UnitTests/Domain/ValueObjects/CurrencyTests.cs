using EnterpriseMediator.Financial.Domain.ValueObjects;
using FluentAssertions;

namespace EnterpriseMediator.Financial.UnitTests.Domain.ValueObjects;

public class CurrencyTests
{
    [Fact]
    public void FromCode_WithValidCode_ShouldCreateCurrency()
    {
        var currency = Currency.FromCode("USD");
        currency.Code.Should().Be("USD");
    }

    [Fact]
    public void FromCode_WithLowerCase_ShouldNormalize()
    {
        var currency = Currency.FromCode("eur");
        currency.Code.Should().Be("EUR");
    }

    [Fact]
    public void FromCode_WithEmptyString_ShouldThrow()
    {
        var act = () => Currency.FromCode("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FromCode_WithInvalidLength_ShouldThrow()
    {
        var act = () => Currency.FromCode("US");
        act.Should().Throw<ArgumentException>().WithMessage("*3 characters*");
    }

    [Fact]
    public void StaticInstances_ShouldHaveCorrectCodes()
    {
        Currency.USD.Code.Should().Be("USD");
        Currency.EUR.Code.Should().Be("EUR");
        Currency.GBP.Code.Should().Be("GBP");
    }

    [Fact]
    public void Equality_SameCode_ShouldBeEqual()
    {
        var a = Currency.FromCode("USD");
        var b = Currency.FromCode("USD");
        a.Should().Be(b);
    }

    [Fact]
    public void ImplicitStringConversion_ShouldReturnCode()
    {
        string code = Currency.USD;
        code.Should().Be("USD");
    }
}
