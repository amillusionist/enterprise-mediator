using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.ValueObjects;
using EnterpriseMediator.Financial.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace EnterpriseMediator.Financial.IntegrationTests.Infrastructure;

public class FinancialRepositoryTests : IClassFixture<FinancialDbContextFixture>
{
    private readonly FinancialDbContextFixture _fixture;

    public FinancialRepositoryTests(FinancialDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAndRetrieveInvoice_ShouldPersistAndReturn()
    {
        await using var context = _fixture.CreateContext();
        var repository = new FinancialRepository(context);

        var invoice = Invoice.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(5000m, Currency.USD));
        invoice.SetPaymentIntent("pi_integration_test");

        await repository.AddInvoiceAsync(invoice, CancellationToken.None);
        await repository.UnitOfWork.SaveChangesAsync(CancellationToken.None);

        // Retrieve from a fresh context to verify persistence
        await using var readContext = _fixture.CreateContext();
        var readRepo = new FinancialRepository(readContext);
        var retrieved = await readRepo.GetInvoiceByIdAsync(invoice.Id, CancellationToken.None);

        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(invoice.Id);
        retrieved.TotalAmount.Amount.Should().Be(5000m);
        retrieved.TotalAmount.Currency.Code.Should().Be("USD");
        retrieved.StripePaymentIntentId.Should().Be("pi_integration_test");
    }

    [Fact]
    public async Task GetInvoiceByPaymentIntentId_ShouldFindCorrectInvoice()
    {
        await using var context = _fixture.CreateContext();
        var repository = new FinancialRepository(context);

        var invoice = Invoice.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(3000m, Currency.EUR));
        invoice.SetPaymentIntent("pi_lookup_test");

        await repository.AddInvoiceAsync(invoice, CancellationToken.None);
        await repository.UnitOfWork.SaveChangesAsync(CancellationToken.None);

        var result = await repository.GetInvoiceByPaymentIntentIdAsync("pi_lookup_test", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(invoice.Id);
    }

    [Fact]
    public async Task AddAndRetrievePayout_ShouldPersistAndReturn()
    {
        await using var context = _fixture.CreateContext();
        var repository = new FinancialRepository(context);

        var payout = Payout.Initiate(Guid.NewGuid(), Guid.NewGuid(), new Money(2000m, Currency.GBP));

        await repository.AddPayoutAsync(payout, CancellationToken.None);
        await repository.UnitOfWork.SaveChangesAsync(CancellationToken.None);

        var retrieved = await repository.GetPayoutByIdAsync(payout.Id, CancellationToken.None);

        retrieved.Should().NotBeNull();
        retrieved!.Amount.Amount.Should().Be(2000m);
        retrieved.Status.Should().Be(PayoutStatus.PendingApproval);
    }

    [Fact]
    public async Task GetPendingPayouts_ShouldReturnOnlyPending()
    {
        await using var context = _fixture.CreateContext();
        var repository = new FinancialRepository(context);

        var pending1 = Payout.Initiate(Guid.NewGuid(), Guid.NewGuid(), new Money(1000m, Currency.USD));
        var pending2 = Payout.Initiate(Guid.NewGuid(), Guid.NewGuid(), new Money(2000m, Currency.USD));
        var approved = Payout.Initiate(Guid.NewGuid(), Guid.NewGuid(), new Money(3000m, Currency.USD));
        approved.Approve(Guid.NewGuid());

        await repository.AddPayoutAsync(pending1, CancellationToken.None);
        await repository.AddPayoutAsync(pending2, CancellationToken.None);
        await repository.AddPayoutAsync(approved, CancellationToken.None);
        await repository.UnitOfWork.SaveChangesAsync(CancellationToken.None);

        var pendingPayouts = (await repository.GetPendingPayoutsAsync(CancellationToken.None)).ToList();

        pendingPayouts.Should().Contain(p => p.Id == pending1.Id);
        pendingPayouts.Should().Contain(p => p.Id == pending2.Id);
        pendingPayouts.Should().NotContain(p => p.Id == approved.Id);
    }

    [Fact]
    public async Task AddTransactionAndCheckIdempotency_ShouldWork()
    {
        await using var context = _fixture.CreateContext();
        var repository = new FinancialRepository(context);

        var invoice = Invoice.Create(Guid.NewGuid(), Guid.NewGuid(), new Money(8000m, Currency.USD));
        invoice.SetPaymentIntent("pi_txn_test");
        await repository.AddInvoiceAsync(invoice, CancellationToken.None);

        var transaction = Transaction.RecordPayment(invoice, "ext_txn_unique_001");
        await repository.AddTransactionAsync(transaction, CancellationToken.None);
        await repository.UnitOfWork.SaveChangesAsync(CancellationToken.None);

        var exists = await repository.ExistsTransactionWithExternalIdAsync("ext_txn_unique_001", CancellationToken.None);
        var notExists = await repository.ExistsTransactionWithExternalIdAsync("ext_txn_nonexistent", CancellationToken.None);

        exists.Should().BeTrue();
        notExists.Should().BeFalse();
    }

    [Fact]
    public async Task GetTransactionsByProjectId_ShouldReturnMatchingTransactions()
    {
        await using var context = _fixture.CreateContext();
        var repository = new FinancialRepository(context);

        var projectId = Guid.NewGuid();
        var invoice = Invoice.Create(projectId, Guid.NewGuid(), new Money(10000m, Currency.USD));
        invoice.SetPaymentIntent($"pi_project_test_{projectId}");
        await repository.AddInvoiceAsync(invoice, CancellationToken.None);

        var txn1 = Transaction.RecordPayment(invoice, $"ext_proj_001_{projectId}");
        var fee = Transaction.RecordFee(projectId, new Money(500m, Currency.USD), $"fee_proj_{projectId}");
        await repository.AddTransactionAsync(txn1, CancellationToken.None);
        await repository.AddTransactionAsync(fee, CancellationToken.None);
        await repository.UnitOfWork.SaveChangesAsync(CancellationToken.None);

        var transactions = (await repository.GetTransactionsByProjectIdAsync(projectId, CancellationToken.None)).ToList();

        transactions.Should().HaveCount(2);
        transactions.Should().Contain(t => t.ProjectId == projectId);
    }
}
