using EnterpriseMediator.Financial.Application.Common.Models;
using EnterpriseMediator.Financial.Application.DTOs;
using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Enums;
using EnterpriseMediator.Financial.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.Financial.Application.Features.Financials.Queries.GetFinancialSummary;

public class GetFinancialSummaryHandler : IRequestHandler<GetFinancialSummaryQuery, Result<FinancialSummaryDto>>
{
    private readonly IFinancialRepository _repository;
    private readonly ILogger<GetFinancialSummaryHandler> _logger;

    public GetFinancialSummaryHandler(
        IFinancialRepository repository,
        ILogger<GetFinancialSummaryHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<FinancialSummaryDto>> Handle(GetFinancialSummaryQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating financial summary for Project {ProjectId}", request.ProjectId);

        var invoice = await _repository.GetInvoiceByProjectIdAsync(request.ProjectId, cancellationToken);
        var transactions = await _repository.GetTransactionsByProjectIdAsync(request.ProjectId, cancellationToken);
        var transactionList = transactions.ToList();

        var totalInvoiced = invoice?.TotalAmount.Amount ?? 0m;
        var currency = invoice?.TotalAmount.Currency.Code ?? "USD";
        var totalPaid = transactionList
            .Where(t => t.Type == TransactionType.ClientPayment)
            .Sum(t => t.Amount.Amount);
        var pendingPayouts = transactionList
            .Where(t => t.Type == TransactionType.VendorPayout)
            .Sum(t => t.Amount.Amount);
        var hasOverdue = invoice?.Status == InvoiceStatus.Overdue;

        var summary = new FinancialSummaryDto
        {
            ProjectId = request.ProjectId,
            TotalBudget = totalInvoiced,
            TotalInvoiced = totalInvoiced,
            TotalPaid = totalPaid,
            PendingPayouts = pendingPayouts,
            Currency = currency,
            HasOverdueInvoices = hasOverdue
        };

        return Result<FinancialSummaryDto>.Success(summary);
    }
}
