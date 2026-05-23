using EnterpriseMediator.Financial.Domain.ValueObjects;
using FluentAssertions;

namespace EnterpriseMediator.Financial.UnitTests.Domain.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Constructor_ShouldCreateMoneyWithAmountAndCurrency()
    {
        var money = new Money(100.50m, Currency.USD);

        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be(Currency.USD);
    }

    [Fact]
    public void Constructor_WithNullCurrency_ShouldThrow()
    {
        var act = () => new Money(100, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void From_ShouldCreateMoneyFromDecimalAndString()
    {
        var money = Money.From(250.75m, "EUR");

        money.Amount.Should().Be(250.75m);
        money.Currency.Code.Should().Be("EUR");
    }

    [Fact]
    public void Zero_ShouldCreateZeroAmountMoney()
    {
        var money = Money.Zero(Currency.GBP);

        money.Amount.Should().Be(0);
        money.Currency.Should().Be(Currency.GBP);
    }

    [Fact]
    public void Addition_SameCurrency_ShouldWork()
    {
        var a = new Money(100m, Currency.USD);
        var b = new Money(50m, Currency.USD);

        var result = a + b;

        result.Amount.Should().Be(150m);
        result.Currency.Should().Be(Currency.USD);
    }

    [Fact]
    public void Addition_DifferentCurrency_ShouldThrow()
    {
        var a = new Money(100m, Currency.USD);
        var b = new Money(50m, Currency.EUR);

        var act = () => a + b;
        act.Should().Throw<InvalidOperationException>().WithMessage("*Currency mismatch*");
    }

    [Fact]
    public void Subtraction_ShouldWork()
    {
        var a = new Money(100m, Currency.USD);
        var b = new Money(30m, Currency.USD);

        var result = a - b;

        result.Amount.Should().Be(70m);
    }

    [Fact]
    public void Multiplication_ShouldWork()
    {
        var money = new Money(100m, Currency.USD);

        var result = money * 1.5m;

        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void Division_ShouldWork()
    {
        var money = new Money(100m, Currency.USD);

        var result = money / 4m;

        result.Amount.Should().Be(25m);
    }

    [Fact]
    public void Division_ByZero_ShouldThrow()
    {
        var money = new Money(100m, Currency.USD);

        var act = () => money / 0m;
        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void Comparison_GreaterThan_ShouldWork()
    {
        var a = new Money(100m, Currency.USD);
        var b = new Money(50m, Currency.USD);

        (a > b).Should().BeTrue();
        (b > a).Should().BeFalse();
    }

    [Fact]
    public void Equality_SameMoney_ShouldBeEqual()
    {
        var a = new Money(100m, Currency.USD);
        var b = new Money(100m, Currency.USD);

        a.Should().Be(b);
    }

    [Fact]
    public void Equality_DifferentAmount_ShouldNotBeEqual()
    {
        var a = new Money(100m, Currency.USD);
        var b = new Money(200m, Currency.USD);

        a.Should().NotBe(b);
    }

    [Fact]
    public void ToString_ShouldFormatCorrectly()
    {
        var money = new Money(1234.56m, Currency.USD);

        money.ToString().Should().Be("1,234.56 USD");
    }
}
