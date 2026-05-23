using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Enums;
using EnterpriseMediator.Financial.Domain.ValueObjects;
using EnterpriseMediator.Financial.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace EnterpriseMediator.Financial.IntegrationTests.Infrastructure;

public class TransactionQueryRepositoryTests : IClassFixture<FinancialDbContextFixture>
{
    private readonly FinancialDbContextFixture _fixture;

    public TransactionQueryRepositoryTests(FinancialDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetPagedTransactions_ShouldReturnPaginatedResults()
    {
        await using var context = _fixture.CreateContext();
        var financialRepo = new FinancialRepository(context);
        var queryRepo = new TransactionQueryRepository(context);

        var projectId = Guid.NewGuid();
        var invoice = Invoice.Create(projectId, Guid.NewGuid(), new Money(10000m, Currency.USD));
        invoice.SetPaymentIntent($"pi_paged_{projectId}");
        await financialRepo.AddInvoiceAsync(invoice, CancellationToken.None);

        // Create multiple transactions
        for (int i = 0; i < 5; i++)
        {
            var txn = Transaction.RecordFee(projectId, new Money(100m * (i + 1), Currency.USD), $"paged_ref_{projectId}_{i}");
            await financialRepo.AddTransactionAsync(txn, CancellationToken.None);
        }
        await financialRepo.UnitOfWork.SaveChangesAsync(CancellationToken.None);

        // Query page 1, size 3
        var (items, totalCount) = await queryRepo.GetPagedTransactionsAsync(
            null, null, TransactionType.PlatformFee, projectId, 1, 3, CancellationToken.None);

        items.Should().HaveCount(3);
        totalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetPagedTransactions_WithDateFilter_ShouldFilterCorrectly()
    {
        await using var context = _fixture.CreateContext();
        var financialRepo = new FinancialRepository(context);
        var queryRepo = new TransactionQueryRepository(context);

        var projectId = Guid.NewGuid();

        var txn = Transaction.RecordFee(projectId, new Money(200m, Currency.USD), $"date_test_{projectId}");
        await financialRepo.AddTransactionAsync(txn, CancellationToken.None);
        await financialRepo.UnitOfWork.SaveChangesAsync(CancellationToken.None);

        // Query with future start date should return nothing for this project
        var (items, totalCount) = await queryRepo.GetPagedTransactionsAsync(
            DateTime.UtcNow.AddDays(1), null, null, projectId, 1, 20, CancellationToken.None);

        items.Should().BeEmpty();
        totalCount.Should().Be(0);
    }
}
