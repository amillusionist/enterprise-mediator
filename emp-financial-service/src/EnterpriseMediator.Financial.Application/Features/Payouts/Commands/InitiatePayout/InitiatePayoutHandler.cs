using EnterpriseMediator.Financial.Application.Common.Models;
using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Interfaces;
using EnterpriseMediator.Financial.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.Financial.Application.Features.Payouts.Commands.InitiatePayout;

public class InitiatePayoutHandler : IRequestHandler<InitiatePayoutCommand, Result<Guid>>
{
    private readonly IFinancialRepository _repository;
    private readonly ILogger<InitiatePayoutHandler> _logger;

    public InitiatePayoutHandler(
        IFinancialRepository repository,
        ILogger<InitiatePayoutHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Guid>> Handle(InitiatePayoutCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initiating payout for Vendor {VendorId} on Project {ProjectId}", request.VendorId, request.ProjectId);

        var money = new Money(request.Amount, Currency.FromCode(request.CurrencyCode));
        var payout = Payout.Initiate(request.VendorId, request.ProjectId, money);

        await _repository.AddPayoutAsync(payout, cancellationToken);
        await _repository.UnitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payout {PayoutId} initiated for Vendor {VendorId}", payout.Id, request.VendorId);

        return Result<Guid>.Success(payout.Id);
    }
}
