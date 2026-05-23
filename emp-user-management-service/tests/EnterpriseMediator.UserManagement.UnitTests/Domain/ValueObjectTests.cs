using EnterpriseMediator.UserManagement.Domain.ValueObjects;
using FluentAssertions;

namespace EnterpriseMediator.UserManagement.UnitTests.Domain;

public class ValueObjectTests
{
    [Fact]
    public void Address_Create_ValidInput_ReturnsAddress()
    {
        var address = Address.Create("123 Main St", "New York", "NY", "10001", "US");

        address.Street.Should().Be("123 Main St");
        address.City.Should().Be("New York");
        address.State.Should().Be("NY");
        address.PostalCode.Should().Be("10001");
        address.Country.Should().Be("US");
    }

    [Theory]
    [InlineData("", "City", "US")]
    [InlineData("Street", "", "US")]
    [InlineData("Street", "City", "")]
    public void Address_Create_MissingRequired_ThrowsArgumentException(string street, string city, string country)
    {
        var act = () => Address.Create(street, city, "State", "12345", country);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Address_StructuralEquality_Works()
    {
        var a1 = Address.Create("123 Main St", "New York", "NY", "10001", "US");
        var a2 = Address.Create("123 Main St", "New York", "NY", "10001", "US");

        a1.Should().Be(a2);
    }

    [Fact]
    public void PaymentInfo_Create_ValidInput_ReturnsPaymentInfo()
    {
        var pi = PaymentInfo.Create("Wise", "GB123456789", "GBP", "SWIFT123");

        pi.ProviderName.Should().Be("Wise");
        pi.AccountIdentifier.Should().Be("GB123456789");
        pi.Currency.Should().Be("GBP");
        pi.RoutingIdentifier.Should().Be("SWIFT123");
    }

    [Fact]
    public void PaymentInfo_GetMaskedDetails_MasksAccountIdentifier()
    {
        var pi = PaymentInfo.Create("Wise", "GB123456789", "GBP");

        var masked = pi.GetMaskedDetails();

        masked.Should().Contain("Wise");
        masked.Should().Contain("6789");
        masked.Should().Contain("GBP");
        masked.Should().NotContain("GB12345");
    }

    [Fact]
    public void PaymentInfo_ToString_ReturnsMasked()
    {
        var pi = PaymentInfo.Create("Wise", "GB123456789", "GBP");

        pi.ToString().Should().Be(pi.GetMaskedDetails());
    }

    [Theory]
    [InlineData("", "ACCT", "USD")]
    [InlineData("Bank", "", "USD")]
    [InlineData("Bank", "ACCT", "")]
    [InlineData("Bank", "ACCT", "ABCD")]
    public void PaymentInfo_InvalidInput_ThrowsArgumentException(string provider, string account, string currency)
    {
        var act = () => PaymentInfo.Create(provider, account, currency);

        act.Should().Throw<ArgumentException>();
    }
}
