using EnterpriseMediator.Financial.Application.DTOs;
using MediatR;

namespace EnterpriseMediator.Financial.Application.Features.Payouts.Queries.GetPendingPayouts;

public record GetPendingPayoutsQuery : IRequest<IReadOnlyList<PayoutDto>>;
