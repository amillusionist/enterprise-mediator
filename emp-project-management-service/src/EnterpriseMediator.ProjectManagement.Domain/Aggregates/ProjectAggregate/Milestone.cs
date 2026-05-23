using EnterpriseMediator.ProjectManagement.Domain.Enums;

namespace EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;

public class Milestone
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = null!;
    public int Order { get; private set; }
    public DateTime? DueDate { get; private set; }
    public MilestoneStatus Status { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public Guid? ApprovedByContactId { get; private set; }
    public DateTime? PaidAt { get; private set; }

    private Milestone() { }

    public Milestone(
        Guid projectId,
        string title,
        string? description,
        decimal amount,
        string currency,
        int order,
        DateTime? dueDate)
    {
        if (projectId == Guid.Empty) throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Milestone title is required.", nameof(title));
        if (amount < 0) throw new ArgumentException("Milestone amount cannot be negative.", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required.", nameof(currency));

        Id = Guid.NewGuid();
        ProjectId = projectId;
        Title = title;
        Description = description;
        Amount = amount;
        Currency = currency.ToUpperInvariant();
        Order = order;
        DueDate = dueDate;
        Status = MilestoneStatus.Pending;
    }

    public void MarkInProgress()
    {
        if (Status != MilestoneStatus.Pending)
            throw new InvalidOperationException($"Cannot start milestone in {Status} state.");
        Status = MilestoneStatus.InProgress;
    }

    public void SubmitForApproval()
    {
        if (Status != MilestoneStatus.InProgress && Status != MilestoneStatus.Rejected)
            throw new InvalidOperationException($"Cannot submit for approval from {Status} state.");
        Status = MilestoneStatus.PendingApproval;
    }

    public void Approve(Guid contactId)
    {
        if (Status != MilestoneStatus.PendingApproval)
            throw new InvalidOperationException($"Cannot approve milestone in {Status} state.");
        Status = MilestoneStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        ApprovedByContactId = contactId;
    }

    public void Reject()
    {
        if (Status != MilestoneStatus.PendingApproval)
            throw new InvalidOperationException($"Cannot reject milestone in {Status} state.");
        Status = MilestoneStatus.Rejected;
    }

    public void MarkAsPaid()
    {
        if (Status != MilestoneStatus.Approved)
            throw new InvalidOperationException($"Cannot mark as paid from {Status} state.");
        PaidAt = DateTime.UtcNow;
    }
}
