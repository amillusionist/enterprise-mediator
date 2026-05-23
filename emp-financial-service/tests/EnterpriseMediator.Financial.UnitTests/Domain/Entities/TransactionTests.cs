using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Enums;
using EnterpriseMediator.Financial.Domain.ValueObjects;
using FluentAssertions;

namespace EnterpriseMediator.Financial.UnitTests.Domain.Entities;

public class TransactionTests
{
    [Fact]
    public void RecordPayment_ShouldCreateClientPaymentTransaction()
    {
        var invoice = Invoice.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(1000m, Currency.USD));
        invoice.SetPaymentIntent("pi_123");

        var transaction = Transaction.RecordPayment(invoice, "ext_txn_001");

        transaction.Should().NotBeNull();
        transaction.Id.Should().NotBeEmpty();
        transaction.Type.Should().Be(TransactionType.ClientPayment);
        transaction.Amount.Should().Be(invoice.TotalAmount);
        transaction.ProjectId.Should().Be(invoice.ProjectId);
        transaction.InvoiceId.Should().Be(invoice.Id);
        transaction.ExternalReferenceId.Should().Be("ext_txn_001");
        transaction.PayoutId.Should().BeNull();
    }

    [Fact]
    public void RecordPayment_WithNullInvoice_ShouldThrow()
    {
        var act = () => Transaction.RecordPayment(null!, "ext_txn_001");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RecordPayout_ShouldCreateVendorPayoutTransaction()
    {
        var payout = Payout.Initiate(Guid.NewGuid(), Guid.NewGuid(), new Money(3000m, Currency.EUR));

        var transaction = Transaction.RecordPayout(payout, "wise_transfer_456");

        transaction.Type.Should().Be(TransactionType.VendorPayout);
        transaction.Amount.Should().Be(payout.Amount);
        transaction.ProjectId.Should().Be(payout.ProjectId);
        transaction.PayoutId.Should().Be(payout.Id);
        transaction.InvoiceId.Should().BeNull();
    }

    [Fact]
    public void RecordRefund_ShouldCreateRefundTransaction()
    {
        var projectId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var amount = new Money(500m, Currency.USD);

        var transaction = Transaction.RecordRefund(projectId, amount, invoiceId, "refund_001", "Client requested cancellation");

        transaction.Type.Should().Be(TransactionType.Refund);
        transaction.Amount.Should().Be(amount);
        transaction.ProjectId.Should().Be(projectId);
        transaction.InvoiceId.Should().Be(invoiceId);
        transaction.Description.Should().Contain("Client requested cancellation");
    }

    [Fact]
    public void RecordFee_ShouldCreatePlatformFeeTransaction()
    {
        var projectId = Guid.NewGuid();
        var amount = new Money(150m, Currency.USD);

        var transaction = Transaction.RecordFee(projectId, amount, "fee_ref_001");

        transaction.Type.Should().Be(TransactionType.PlatformFee);
        transaction.Amount.Should().Be(amount);
        transaction.Description.Should().Contain("Platform Service Fee");
    }
}
