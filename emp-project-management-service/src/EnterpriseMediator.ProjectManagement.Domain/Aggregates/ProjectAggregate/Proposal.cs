using EnterpriseMediator.ProjectManagement.Domain.Enums;

namespace EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;

public class Proposal
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid VendorId { get; private set; }
    public decimal ProposedCost { get; private set; }
    public string Currency { get; private set; } = null!;
    public string Timeline { get; private set; } = null!;
    public string KeyPersonnel { get; private set; } = null!;
    public string CoverLetter { get; private set; } = string.Empty;
    public string? ProposalDocumentUrl { get; private set; }
    public ProposalStatus Status { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }
    public int? InternalScore { get; private set; }
    public string? InternalFlag { get; private set; }

    private Proposal() { }

    public Proposal(
        Guid projectId,
        Guid vendorId,
        decimal proposedCost,
        string currency,
        string timeline,
        string keyPersonnel,
        string coverLetter,
        string? proposalDocumentUrl)
    {
        if (projectId == Guid.Empty) throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));
        if (vendorId == Guid.Empty) throw new ArgumentException("Vendor ID cannot be empty.", nameof(vendorId));
        if (proposedCost < 0) throw new ArgumentException("Proposed cost cannot be negative.", nameof(proposedCost));
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency code is required.", nameof(currency));
        if (string.IsNullOrWhiteSpace(timeline)) throw new ArgumentException("Timeline is required.", nameof(timeline));
        if (string.IsNullOrWhiteSpace(keyPersonnel)) throw new ArgumentException("Key personnel information is required.", nameof(keyPersonnel));

        Id = Guid.NewGuid();
        ProjectId = projectId;
        VendorId = vendorId;
        ProposedCost = proposedCost;
        Currency = currency.ToUpperInvariant();
        Timeline = timeline;
        KeyPersonnel = keyPersonnel;
        CoverLetter = coverLetter ?? string.Empty;
        ProposalDocumentUrl = proposalDocumentUrl;
        Status = ProposalStatus.Submitted;
        SubmittedAt = DateTimeOffset.UtcNow;
    }

    public void MarkAsInReview()
    {
        if (Status != ProposalStatus.Submitted)
            throw new InvalidOperationException($"Cannot review proposal in {Status} state.");
        Status = ProposalStatus.InReview;
    }

    public void Shortlist()
    {
        if (Status is ProposalStatus.Rejected or ProposalStatus.Withdrawn or ProposalStatus.Accepted)
            throw new InvalidOperationException($"Cannot shortlist a proposal that is {Status}.");
        Status = ProposalStatus.Shortlisted;
    }

    public void Accept()
    {
        if (Status == ProposalStatus.Withdrawn)
            throw new InvalidOperationException("Cannot accept a withdrawn proposal.");
        Status = ProposalStatus.Accepted;
    }

    public void Reject()
    {
        if (Status == ProposalStatus.Accepted)
            throw new InvalidOperationException("Cannot reject an already accepted proposal.");
        Status = ProposalStatus.Rejected;
    }

    public void Withdraw()
    {
        if (Status == ProposalStatus.Accepted)
            throw new InvalidOperationException("Cannot withdraw an accepted proposal.");
        Status = ProposalStatus.Withdrawn;
    }

    public void UpdateAssessment(int? score, string? flag)
    {
        if (score.HasValue && (score.Value < 1 || score.Value > 5))
            throw new ArgumentOutOfRangeException(nameof(score), "Internal score must be between 1 and 5.");
        InternalScore = score;
        InternalFlag = flag;
    }
}
