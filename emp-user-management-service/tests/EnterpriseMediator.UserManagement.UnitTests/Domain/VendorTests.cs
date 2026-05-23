using EnterpriseMediator.UserManagement.Domain.Aggregates.Vendor;
using EnterpriseMediator.UserManagement.Domain.Events;
using EnterpriseMediator.UserManagement.Domain.ValueObjects;
using FluentAssertions;

namespace EnterpriseMediator.UserManagement.UnitTests.Domain;

public class VendorTests
{
    private static Address CreateTestAddress() =>
        Address.Create("123 Main St", "New York", "NY", "10001", "US");

    [Fact]
    public void Create_ValidInput_ReturnsVendorWithCorrectProperties()
    {
        var vendor = Vendor.Create("Acme Corp", CreateTestAddress(), "contact@acme.com");

        vendor.CompanyName.Should().Be("Acme Corp");
        vendor.PrimaryContactEmail.Should().Be("contact@acme.com");
        vendor.VettingStatus.Should().Be("Pending");
        vendor.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_RaisesVendorCreatedEvent()
    {
        var vendor = Vendor.Create("Acme Corp", CreateTestAddress(), "contact@acme.com");

        vendor.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<VendorCreatedEvent>();
    }

    [Fact]
    public void UpdateSkills_AddsNewSkills()
    {
        var vendor = Vendor.Create("Acme Corp", CreateTestAddress(), "contact@acme.com");
        vendor.ClearDomainEvents();

        vendor.UpdateSkills(new[] { "C#", ".NET", "Azure" });

        vendor.Skills.Should().HaveCount(3);
        vendor.Skills.Select(s => s.Name).Should().Contain("C#");
    }

    [Fact]
    public void UpdateSkills_RemovesMissingAndAddsNew()
    {
        var vendor = Vendor.Create("Acme Corp", CreateTestAddress(), "contact@acme.com");
        vendor.UpdateSkills(new[] { "C#", ".NET", "Azure" });
        vendor.ClearDomainEvents();

        vendor.UpdateSkills(new[] { "C#", "AWS", "React" });

        vendor.Skills.Should().HaveCount(3);
        vendor.Skills.Select(s => s.Name).Should().Contain("C#");
        vendor.Skills.Select(s => s.Name).Should().Contain("AWS");
        vendor.Skills.Select(s => s.Name).Should().NotContain("Azure");
    }

    [Fact]
    public void UpdateSkills_RaisesProfileUpdatedEvent()
    {
        var vendor = Vendor.Create("Acme Corp", CreateTestAddress(), "contact@acme.com");
        vendor.ClearDomainEvents();

        vendor.UpdateSkills(new[] { "C#" });

        vendor.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<VendorProfileUpdated>()
            .Which.SkillsUpdated.Should().BeTrue();
    }

    [Fact]
    public void UpdatePaymentDetails_SetsPaymentInfo()
    {
        var vendor = Vendor.Create("Acme Corp", CreateTestAddress(), "contact@acme.com");
        var payment = PaymentInfo.Create("Wise", "GB1234567890", "GBP", "SWIFT123");

        vendor.UpdatePaymentDetails(payment);

        vendor.PaymentDetails.Should().NotBeNull();
        vendor.PaymentDetails!.ProviderName.Should().Be("Wise");
        vendor.PaymentDetails.Currency.Should().Be("GBP");
    }

    [Fact]
    public void ChangeVettingStatus_UpdatesStatusAndRaisesEvent()
    {
        var vendor = Vendor.Create("Acme Corp", CreateTestAddress(), "contact@acme.com");
        vendor.ClearDomainEvents();

        vendor.ChangeVettingStatus("Active");

        vendor.VettingStatus.Should().Be("Active");
        vendor.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<VendorVettingStatusChangedEvent>();
    }

    [Fact]
    public void ChangeVettingStatus_SameStatus_NoEventRaised()
    {
        var vendor = Vendor.Create("Acme Corp", CreateTestAddress(), "contact@acme.com");
        vendor.ClearDomainEvents();

        vendor.ChangeVettingStatus("Pending");

        vendor.DomainEvents.Should().BeEmpty();
    }
}
