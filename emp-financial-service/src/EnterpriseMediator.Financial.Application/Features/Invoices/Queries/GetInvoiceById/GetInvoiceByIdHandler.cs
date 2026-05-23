using EnterpriseMediator.Financial.Application.Common.Models;
using EnterpriseMediator.Financial.Application.DTOs;
using EnterpriseMediator.Financial.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.Financial.Application.Features.Invoices.Queries.GetInvoiceById;

public class GetInvoiceByIdHandler : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDto>>
{
    private readonly IFinancialRepository _repository;
    private readonly ILogger<GetInvoiceByIdHandler> _logger;

    public GetInvoiceByIdHandler(
        IFinancialRepository repository,
        ILogger<GetInvoiceByIdHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving invoice {InvoiceId}", request.InvoiceId);

        var invoice = await _repository.GetInvoiceByIdAsync(request.InvoiceId, cancellationToken);

        if (invoice is null)
        {
            _logger.LogWarning("Invoice {InvoiceId} not found", request.InvoiceId);
            return Result<InvoiceDto>.Failure($"Invoice {request.InvoiceId} not found.");
        }

        var dto = new InvoiceDto
        {
            Id = invoice.Id,
            ProjectId = invoice.ProjectId,
            ClientId = invoice.ClientId,
            Amount = invoice.TotalAmount.Amount,
            Currency = invoice.TotalAmount.Currency.Code,
            Status = invoice.Status.ToString(),
            StripePaymentIntentId = invoice.StripePaymentIntentId,
            CreatedAt = invoice.CreatedAt,
            PaidAt = invoice.PaidAt
        };

        return Result<InvoiceDto>.Success(dto);
    }
}
