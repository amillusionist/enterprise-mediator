using EnterpriseMediator.Financial.Application.DTOs;
using EnterpriseMediator.Financial.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.Financial.Application.Features.Payouts.Queries.GetPendingPayouts;

public class GetPendingPayoutsHandler : IRequestHandler<GetPendingPayoutsQuery, IReadOnlyList<PayoutDto>>
{
    private readonly IFinancialRepository _repository;
    private readonly ILogger<GetPendingPayoutsHandler> _logger;

    public GetPendingPayoutsHandler(
        IFinancialRepository repository,
        ILogger<GetPendingPayoutsHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<PayoutDto>> Handle(GetPendingPayoutsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving pending payouts");

        var payouts = await _repository.GetPendingPayoutsAsync(cancellationToken);

        return payouts.Select(p => new PayoutDto
        {
            Id = p.Id,
            VendorId = p.VendorId,
            ProjectId = p.ProjectId,
            Amount = p.Amount.Amount,
            Currency = p.Amount.Currency.Code,
            Status = p.Status.ToString(),
            WiseTransferId = p.WiseTransferId,
            ApproverId = p.ApproverId,
            CreatedAt = p.CreatedAt,
            ProcessedAt = p.ProcessedAt,
            FailureReason = p.FailureReason
        }).ToList();
    }
}
