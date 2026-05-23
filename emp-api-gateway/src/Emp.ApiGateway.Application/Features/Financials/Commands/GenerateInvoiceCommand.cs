using EnterpriseMediator.Contracts.DTOs.Financials;
using MediatR;

namespace Emp.ApiGateway.Application.Features.Financials.Commands;

public record GenerateInvoiceCommand : IRequest<InvoiceDto>
{
    public required Guid ProjectId { get; init; }
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public DateTimeOffset? DueDate { get; init; }
    public string? Description { get; init; }
}
