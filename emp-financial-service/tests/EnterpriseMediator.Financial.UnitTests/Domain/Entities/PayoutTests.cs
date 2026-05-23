using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Events;
using EnterpriseMediator.Financial.Domain.ValueObjects;
using FluentAssertions;

namespace EnterpriseMediator.Financial.UnitTests.Domain.Entities;

public class PayoutTests
{
    private static readonly Money ValidAmount = new(5000.00m, Currency.USD);
    private static readonly Guid ValidVendorId = Guid.NewGuid();
    private static readonly Guid ValidProjectId = Guid.NewGuid();

    [Fact]
    public void Initiate_WithValidInputs_ShouldCreatePendingPayout()
    {
        var payout = Payout.Initiate(ValidVendorId, ValidProjectId, ValidAmount);

        payout.Should().NotBeNull();
        payout.Id.Should().NotBeEmpty();
        payout.VendorId.Should().Be(ValidVendorId);
        payout.ProjectId.Should().Be(ValidProjectId);
        payout.Amount.Should().Be(ValidAmount);
        payout.Status.Should().Be(PayoutStatus.PendingApproval);
    }

    [Fact]
    public void Initiate_WithEmptyVendorId_ShouldThrow()
    {
        var act = () => Payout.Initiate(Guid.Empty, ValidProjectId, ValidAmount);
        act.Should().Throw<ArgumentException>().WithParameterName("vendorId");
    }

    [Fact]
    public void Initiate_WithZeroAmount_ShouldThrow()
    {
        var act = () => Payout.Initiate(ValidVendorId, ValidProjectId, new Money(0, Currency.USD));
        act.Should().Throw<ArgumentException>().WithParameterName("amount");
    }

    [Fact]
    public void Approve_WhenPending_ShouldTransitionToApproved()
    {
        var payout = Payout.Initiate(ValidVendorId, ValidProjectId, ValidAmount);
        var approverId = Guid.NewGuid();

        payout.Approve(approverId);

        payout.Status.Should().Be(PayoutStatus.Approved);
        payout.ApproverId.Should().Be(approverId);
    }

    [Fact]
    public void Approve_WhenNotPending_ShouldThrow()
    {
        var payout = Payout.Initiate(ValidVendorId, ValidProjectId, ValidAmount);
        payout.Approve(Guid.NewGuid());

        var act = () => payout.Approve(Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkAsProcessing_WhenApproved_ShouldTransitionToProcessing()
    {
        var payout = Payout.Initiate(ValidVendorId, ValidProjectId, ValidAmount);
        payout.Approve(Guid.NewGuid());

        payout.MarkAsProcessing("wise_transfer_123");

        payout.Status.Should().Be(PayoutStatus.Processing);
        payout.WiseTransferId.Should().Be("wise_transfer_123");
    }

    [Fact]
    public void MarkAsProcessing_WhenNotApproved_ShouldThrow()
    {
        var payout = Payout.Initiate(ValidVendorId, ValidProjectId, ValidAmount);

        var act = () => payout.MarkAsProcessing("wise_transfer_123");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkAsPaid_WhenProcessing_ShouldTransitionToPaidAndRaiseEvent()
    {
        var payout = Payout.Initiate(ValidVendorId, ValidProjectId, ValidAmount);
        payout.Approve(Guid.NewGuid());
        payout.MarkAsProcessing("wise_transfer_123");
        var processedAt = DateTime.UtcNow;

        payout.MarkAsPaid(processedAt);

        payout.Status.Should().Be(PayoutStatus.Paid);
        payout.ProcessedAt.Should().Be(processedAt);
        payout.DomainEvents.Should().HaveCount(1);
        payout.DomainEvents.First().Should().BeOfType<PayoutProcessedEvent>();
    }

    [Fact]
    public void MarkAsFailed_ShouldTransitionToFailed()
    {
        var payout = Payout.Initiate(ValidVendorId, ValidProjectId, ValidAmount);
        payout.Approve(Guid.NewGuid());
        payout.MarkAsProcessing("wise_transfer_123");

        payout.MarkAsFailed("Insufficient funds");

        payout.Status.Should().Be(PayoutStatus.Failed);
        payout.FailureReason.Should().Be("Insufficient funds");
    }

    [Fact]
    public void Reject_WhenPending_ShouldTransitionToRejected()
    {
        var payout = Payout.Initiate(ValidVendorId, ValidProjectId, ValidAmount);
        var rejectorId = Guid.NewGuid();

        payout.Reject(rejectorId, "Budget exceeded");

        payout.Status.Should().Be(PayoutStatus.Rejected);
        payout.FailureReason.Should().Be("Budget exceeded");
    }

    [Fact]
    public void Reject_WhenNotPending_ShouldThrow()
    {
        var payout = Payout.Initiate(ValidVendorId, ValidProjectId, ValidAmount);
        payout.Approve(Guid.NewGuid());

        var act = () => payout.Reject(Guid.NewGuid(), "Too late");
        act.Should().Throw<InvalidOperationException>();
    }
}
