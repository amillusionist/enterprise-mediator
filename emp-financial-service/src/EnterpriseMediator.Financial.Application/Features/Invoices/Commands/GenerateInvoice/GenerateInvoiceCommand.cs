using EnterpriseMediator.Financial.Application.Common.Models;
using MediatR;

namespace EnterpriseMediator.Financial.Application.Features.Invoices.Commands.GenerateInvoice;

public record GenerateInvoiceCommand(
    Guid ProjectId,
    Guid ClientId,
    decimal Amount,
    string CurrencyCode) : IRequest<Result<Guid>>;
