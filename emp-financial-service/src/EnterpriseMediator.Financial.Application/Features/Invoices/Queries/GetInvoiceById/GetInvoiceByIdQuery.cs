using EnterpriseMediator.Financial.Application.Common.Models;
using EnterpriseMediator.Financial.Application.DTOs;
using MediatR;

namespace EnterpriseMediator.Financial.Application.Features.Invoices.Queries.GetInvoiceById;

public record GetInvoiceByIdQuery(Guid InvoiceId) : IRequest<Result<InvoiceDto>>;
