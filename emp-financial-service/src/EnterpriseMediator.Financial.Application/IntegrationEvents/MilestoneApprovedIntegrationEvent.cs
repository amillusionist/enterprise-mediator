namespace EnterpriseMediator.Financial.Application.IntegrationEvents;

public record MilestoneApprovedIntegrationEvent
{
    public Guid ProjectId { get; init; }
    public Guid MilestoneId { get; init; }
    public decimal PayoutAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public Guid VendorId { get; init; }
}
