using EnterpriseMediator.Financial.Application.Common.Models;
using MediatR;

namespace EnterpriseMediator.Financial.Application.Features.Payouts.Commands.InitiatePayout;

public record InitiatePayoutCommand(
    Guid VendorId,
    Guid ProjectId,
    decimal Amount,
    string CurrencyCode) : IRequest<Result<Guid>>;
