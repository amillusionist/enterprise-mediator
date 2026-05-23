using EnterpriseMediator.Financial.Application.Common.Models;
using EnterpriseMediator.Financial.Application.DTOs;
using MediatR;

namespace EnterpriseMediator.Financial.Application.Features.Financials.Queries.GetFinancialSummary;

public record GetFinancialSummaryQuery(Guid ProjectId) : IRequest<Result<FinancialSummaryDto>>;
