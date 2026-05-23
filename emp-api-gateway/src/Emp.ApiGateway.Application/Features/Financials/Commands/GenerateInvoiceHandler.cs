using Emp.ApiGateway.Application.Interfaces.Infrastructure;
using EnterpriseMediator.Contracts.DTOs.Financials;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Emp.ApiGateway.Application.Features.Financials.Commands;

public class GenerateInvoiceHandler : IRequestHandler<GenerateInvoiceCommand, InvoiceDto>
{
    private readonly IFinancialServiceClient _financialService;
    private readonly ILogger<GenerateInvoiceHandler> _logger;

    public GenerateInvoiceHandler(IFinancialServiceClient financialService, ILogger<GenerateInvoiceHandler> logger)
    {
        _financialService = financialService ?? throw new ArgumentNullException(nameof(financialService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<InvoiceDto> Handle(GenerateInvoiceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Generating invoice for ProjectId: {ProjectId}, Amount: {Amount} {Currency}",
            request.ProjectId, request.Amount, request.Currency);

        var invoiceRequest = new GenerateInvoiceRequest
        {
            ProjectId = request.ProjectId,
            Amount = request.Amount,
            Currency = request.Currency,
            DueDate = request.DueDate,
            Description = request.Description
        };

        return await _financialService.GenerateInvoiceAsync(invoiceRequest, cancellationToken);
    }
}
