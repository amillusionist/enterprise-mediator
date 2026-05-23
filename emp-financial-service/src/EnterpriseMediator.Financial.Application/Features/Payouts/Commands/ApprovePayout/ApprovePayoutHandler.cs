using EnterpriseMediator.Financial.Application.Common.Models;
using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.Financial.Application.Features.Payouts.Commands.ApprovePayout;

public class ApprovePayoutHandler : IRequestHandler<ApprovePayoutCommand, Result>
{
    private readonly IFinancialRepository _repository;
    private readonly IPayoutGateway _payoutGateway;
    private readonly ILogger<ApprovePayoutHandler> _logger;

    public ApprovePayoutHandler(
        IFinancialRepository repository,
        IPayoutGateway payoutGateway,
        ILogger<ApprovePayoutHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _payoutGateway = payoutGateway ?? throw new ArgumentNullException(nameof(payoutGateway));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> Handle(ApprovePayoutCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving payout {PayoutId} by {ApproverId}", request.PayoutId, request.ApproverId);

        var payout = await _repository.GetPayoutByIdAsync(request.PayoutId, cancellationToken);
        if (payout is null)
            return Result.Failure($"Payout {request.PayoutId} not found.");

        payout.Approve(request.ApproverId);

        var idempotencyKey = $"payout_{payout.Id}";
        var payoutResult = await _payoutGateway.ExecutePayoutAsync(payout, idempotencyKey, cancellationToken);

        payout.MarkAsProcessing(payoutResult.TransferId);

        var transaction = Transaction.RecordPayout(payout, payoutResult.TransferId);
        await _repository.AddTransactionAsync(transaction, cancellationToken);
        await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payout {PayoutId} approved and submitted. TransferId: {TransferId}",
            payout.Id, payoutResult.TransferId);

        return Result.Success();
    }
}
