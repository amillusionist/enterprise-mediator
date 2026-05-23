using EnterpriseMediator.Financial.Application.Common.Models;
using EnterpriseMediator.Financial.Application.Features.Ledger.DTOs;
using EnterpriseMediator.Financial.Domain.Enums;
using EnterpriseMediator.Financial.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.Financial.Application.Features.Ledger.Queries.GetTransactionHistory;

public class GetTransactionHistoryHandler
    : IRequestHandler<GetTransactionHistoryQuery, PagedResult<TransactionSummaryDto>>
{
    private readonly ITransactionQueryRepository _transactionQueryRepository;
    private readonly ILogger<GetTransactionHistoryHandler> _logger;

    public GetTransactionHistoryHandler(
        ITransactionQueryRepository transactionQueryRepository,
        ILogger<GetTransactionHistoryHandler> logger)
    {
        _transactionQueryRepository = transactionQueryRepository ?? throw new ArgumentNullException(nameof(transactionQueryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PagedResult<TransactionSummaryDto>> Handle(
        GetTransactionHistoryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Querying transaction ledger. Page {Page}, Size {PageSize}", request.Page, request.PageSize);

        TransactionType? parsedType = null;
        if (!string.IsNullOrWhiteSpace(request.TransactionType) &&
            Enum.TryParse<TransactionType>(request.TransactionType, ignoreCase: true, out var type))
        {
            parsedType = type;
        }

        var (transactions, totalCount) = await _transactionQueryRepository.GetPagedTransactionsAsync(
            request.StartDate,
            request.EndDate,
            parsedType,
            request.ProjectId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var dtos = transactions.Select(t => new TransactionSummaryDto
        {
            Id = t.Id,
            Type = t.Type.ToString(),
            Amount = t.Amount.Amount,
            Currency = t.Amount.Currency.Code,
            Timestamp = t.Timestamp,
            ProjectId = t.ProjectId,
            InvoiceId = t.InvoiceId,
            PayoutId = t.PayoutId,
            ExternalReferenceId = t.ExternalReferenceId,
            Description = t.Description
        }).ToList();

        return new PagedResult<TransactionSummaryDto>(dtos, totalCount, request.Page, request.PageSize);
    }
}
