using EnterpriseMediator.Financial.Application.Common.Models;
using MediatR;

namespace EnterpriseMediator.Financial.Application.Features.Payouts.Commands.ApprovePayout;

public record ApprovePayoutCommand(Guid PayoutId, Guid ApproverId) : IRequest<Result>;
