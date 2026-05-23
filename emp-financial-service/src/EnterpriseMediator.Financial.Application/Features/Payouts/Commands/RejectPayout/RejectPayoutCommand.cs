using EnterpriseMediator.Financial.Application.Common.Models;
using MediatR;

namespace EnterpriseMediator.Financial.Application.Features.Payouts.Commands.RejectPayout;

public record RejectPayoutCommand(Guid PayoutId, Guid RejectorId, string Reason) : IRequest<Result>;
