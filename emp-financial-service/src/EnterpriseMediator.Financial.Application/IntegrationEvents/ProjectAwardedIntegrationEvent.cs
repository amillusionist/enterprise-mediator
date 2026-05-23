namespace EnterpriseMediator.Financial.Application.IntegrationEvents;

public record ProjectAwardedIntegrationEvent
{
    public Guid ProjectId { get; init; }
    public Guid VendorId { get; init; }
    public Guid ProposalId { get; init; }
    public decimal ProposedCost { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime AwardedAt { get; init; }
}
