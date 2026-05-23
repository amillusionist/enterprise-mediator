using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Enums;
using EnterpriseMediator.Financial.Domain.Events;
using EnterpriseMediator.Financial.Domain.ValueObjects;
using FluentAssertions;

namespace EnterpriseMediator.Financial.UnitTests.Domain.Entities;

public class InvoiceTests
{
    private static readonly Money ValidAmount = new(1500.00m, Currency.USD);
    private static readonly Guid ValidProjectId = Guid.NewGuid();
    private static readonly Guid ValidClientId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidInputs_ShouldCreateDraftInvoice()
    {
        var invoice = Invoice.Create(ValidProjectId, ValidClientId, ValidAmount);

        invoice.Should().NotBeNull();
        invoice.Id.Should().NotBeEmpty();
        invoice.ProjectId.Should().Be(ValidProjectId);
        invoice.ClientId.Should().Be(ValidClientId);
        invoice.TotalAmount.Should().Be(ValidAmount);
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.PaidAt.Should().BeNull();
        invoice.StripePaymentIntentId.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyProjectId_ShouldThrow()
    {
        var act = () => Invoice.Create(Guid.Empty, ValidClientId, ValidAmount);
        act.Should().Throw<ArgumentException>().WithParameterName("projectId");
    }

    [Fact]
    public void Create_WithEmptyClientId_ShouldThrow()
    {
        var act = () => Invoice.Create(ValidProjectId, Guid.Empty, ValidAmount);
        act.Should().Throw<ArgumentException>().WithParameterName("clientId");
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldThrow()
    {
        var act = () => Invoice.Create(ValidProjectId, ValidClientId, new Money(0, Currency.USD));
        act.Should().Throw<ArgumentException>().WithParameterName("totalAmount");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrow()
    {
        var act = () => Invoice.Create(ValidProjectId, ValidClientId, new Money(-100, Currency.USD));
        act.Should().Throw<ArgumentException>().WithParameterName("totalAmount");
    }

    [Fact]
    public void SetPaymentIntent_OnDraftInvoice_ShouldTransitionToSent()
    {
        var invoice = Invoice.Create(ValidProjectId, ValidClientId, ValidAmount);

        invoice.SetPaymentIntent("pi_test_123");

        invoice.Status.Should().Be(InvoiceStatus.Sent);
        invoice.StripePaymentIntentId.Should().Be("pi_test_123");
    }

    [Fact]
    public void SetPaymentIntent_WithEmptyId_ShouldThrow()
    {
        var invoice = Invoice.Create(ValidProjectId, ValidClientId, ValidAmount);

        var act = () => invoice.SetPaymentIntent("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetPaymentIntent_OnPaidInvoice_ShouldThrow()
    {
        var invoice = Invoice.Create(ValidProjectId, ValidClientId, ValidAmount);
        invoice.SetPaymentIntent("pi_test_123");
        invoice.MarkAsPaid("txn_ref", DateTime.UtcNow);

        var act = () => invoice.SetPaymentIntent("pi_test_456");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkAsPaid_OnSentInvoice_ShouldTransitionToPaid()
    {
        var invoice = Invoice.Create(ValidProjectId, ValidClientId, ValidAmount);
        invoice.SetPaymentIntent("pi_test_123");
        var paidAt = DateTime.UtcNow;

        invoice.MarkAsPaid("txn_ref_001", paidAt);

        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.PaidAt.Should().Be(paidAt);
    }

    [Fact]
    public void MarkAsPaid_ShouldRaiseDomainEvent()
    {
        var invoice = Invoice.Create(ValidProjectId, ValidClientId, ValidAmount);
        invoice.SetPaymentIntent("pi_test_123");

        invoice.MarkAsPaid("txn_ref_001", DateTime.UtcNow);

        invoice.DomainEvents.Should().HaveCount(1);
        invoice.DomainEvents.First().Should().BeOfType<InvoicePaidEvent>();
    }

    [Fact]
    public void MarkAsPaid_WhenAlreadyPaid_ShouldBeIdempotent()
    {
        var invoice = Invoice.Create(ValidProjectId, ValidClientId, ValidAmount);
        invoice.SetPaymentIntent("pi_test_123");
        invoice.MarkAsPaid("txn_ref_001", DateTime.UtcNow);

        invoice.MarkAsPaid("txn_ref_002", DateTime.UtcNow);

        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.DomainEvents.Should().HaveCount(1); // Only one event raised
    }

    [Fact]
    public void MarkAsPaid_OnCancelledInvoice_ShouldThrow()
    {
        var invoice = Invoice.Create(ValidProjectId, ValidClientId, ValidAmount);
        invoice.Cancel();

        var act = () => invoice.MarkAsPaid("txn_ref", DateTime.UtcNow);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_OnDraftInvoice_ShouldTransitionToCancelled()
    {
        var invoice = Invoice.Create(ValidProjectId, ValidClientId, ValidAmount);

        invoice.Cancel();

        invoice.Status.Should().Be(InvoiceStatus.Cancelled);
    }

    [Fact]
    public void Cancel_OnPaidInvoice_ShouldThrow()
    {
        var invoice = Invoice.Create(ValidProjectId, ValidClientId, ValidAmount);
        invoice.SetPaymentIntent("pi_test_123");
        invoice.MarkAsPaid("txn_ref", DateTime.UtcNow);

        var act = () => invoice.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var invoice = Invoice.Create(ValidProjectId, ValidClientId, ValidAmount);
        invoice.SetPaymentIntent("pi_test_123");
        invoice.MarkAsPaid("txn_ref", DateTime.UtcNow);

        invoice.ClearDomainEvents();

        invoice.DomainEvents.Should().BeEmpty();
    }
}
