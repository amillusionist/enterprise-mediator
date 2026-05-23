using EnterpriseMediator.Financial.Application.Common.Models;
using EnterpriseMediator.Financial.Application.Features.Ledger.DTOs;
using MediatR;

namespace EnterpriseMediator.Financial.Application.Features.Ledger.Queries.GetTransactionHistory;

public class GetTransactionHistoryQuery : IRequest<PagedResult<TransactionSummaryDto>>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? TransactionType { get; set; }
    public Guid? ProjectId { get; set; }
    public Guid? ClientId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
