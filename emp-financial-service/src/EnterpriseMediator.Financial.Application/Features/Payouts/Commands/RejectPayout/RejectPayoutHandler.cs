using EnterpriseMediator.Financial.Application.Common.Models;
using EnterpriseMediator.Financial.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.Financial.Application.Features.Payouts.Commands.RejectPayout;

public class RejectPayoutHandler : IRequestHandler<RejectPayoutCommand, Result>
{
    private readonly IFinancialRepository _repository;
    private readonly ILogger<RejectPayoutHandler> _logger;

    public RejectPayoutHandler(
        IFinancialRepository repository,
        ILogger<RejectPayoutHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(RejectPayoutCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Rejecting payout {PayoutId} by {RejectorId}", request.PayoutId, request.RejectorId);

        var payout = await _repository.GetPayoutByIdAsync(request.PayoutId, cancellationToken);
        if (payout is null)
            return Result.Failure($"Payout {request.PayoutId} not found.");

        payout.Reject(request.RejectorId, request.Reason);
        await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payout {PayoutId} rejected. Reason: {Reason}", payout.Id, request.Reason);

        return Result.Success();
    }
}
