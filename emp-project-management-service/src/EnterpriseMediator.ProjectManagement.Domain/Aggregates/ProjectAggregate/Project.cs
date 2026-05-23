using EnterpriseMediator.ProjectManagement.Domain.Enums;
using EnterpriseMediator.ProjectManagement.Domain.Events;

namespace EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;

public class Project
{
    private readonly List<Proposal> _proposals = new();
    private readonly List<Milestone> _milestones = new();
    private readonly List<ProjectPayoutRule> _payoutRules = new();
    private readonly List<IDomainEvent> _domainEvents = new();

    protected Project() { }

    private Project(Guid id, Guid clientId, string name, string description)
    {
        if (id == Guid.Empty) throw new ArgumentException("Project ID cannot be empty.", nameof(id));
        if (clientId == Guid.Empty) throw new ArgumentException("Client ID cannot be empty.", nameof(clientId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Project name cannot be empty.", nameof(name));

        Id = id;
        ClientId = clientId;
        Name = name;
        Description = description ?? string.Empty;
        Status = ProjectStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid ClientId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public ProjectStatus Status { get; private set; }

    public SowDetails? SowDetails { get; private set; }
    public SowDocument? SowDocument { get; private set; }

    public decimal? FixedMargin { get; private set; }
    public decimal? PercentageMargin { get; private set; }

    public Guid? AwardedVendorId { get; private set; }
    public Guid? AwardedProposalId { get; private set; }

    public IReadOnlyCollection<Proposal> Proposals => _proposals.AsReadOnly();
    public IReadOnlyCollection<Milestone> Milestones => _milestones.AsReadOnly();
    public IReadOnlyCollection<ProjectPayoutRule> PayoutRules => _payoutRules.AsReadOnly();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static Project Create(Guid clientId, string name, string description)
    {
        var project = new Project(Guid.NewGuid(), clientId, name, description);
        project.AddDomainEvent(new ProjectCreatedDomainEvent(project.Id, clientId, name));
        return project;
    }

    public SowDocument UploadSow(string fileName, string contentType, long fileSizeBytes, string storageKey, Guid uploadedBy)
    {
        if (Status != ProjectStatus.Pending && Status != ProjectStatus.Failed)
            throw new InvalidOperationException("Cannot upload SOW in current state.");

        var sow = new SowDocument(Id, fileName, contentType, fileSizeBytes, storageKey, uploadedBy);
        sow.MarkProcessing();
        SowDocument = sow;
        var oldStatus = Status;
        Status = ProjectStatus.Processing;
        AddDomainEvent(new SowUploadedDomainEvent(Id, sow.Id, storageKey));
        AddDomainEvent(new ProjectStatusChangedDomainEvent(Id, oldStatus.ToString(), Status.ToString()));
        return sow;
    }

    public void AttachSowDetails(SowDetails sowDetails)
    {
        if (Status != ProjectStatus.Processing)
            throw new InvalidOperationException("Cannot attach SOW details in current state.");

        SowDetails = sowDetails ?? throw new ArgumentNullException(nameof(sowDetails));
        SowDocument?.MarkProcessed();
        var oldStatus = Status;
        Status = ProjectStatus.Processed;
        AddDomainEvent(new ProjectStatusChangedDomainEvent(Id, oldStatus.ToString(), Status.ToString()));
    }

    public void MarkSowFailed(string reason)
    {
        if (Status != ProjectStatus.Processing)
            throw new InvalidOperationException("Cannot mark SOW failed in current state.");

        SowDocument?.MarkFailed(reason);
        var oldStatus = Status;
        Status = ProjectStatus.Failed;
        AddDomainEvent(new ProjectStatusChangedDomainEvent(Id, oldStatus.ToString(), Status.ToString()));
    }

    public void UpdateBrief(SowDetails updatedSowDetails)
    {
        if (Status != ProjectStatus.Processed && Status != ProjectStatus.BriefApproved)
            throw new InvalidOperationException("Cannot update brief in current state.");
        SowDetails = updatedSowDetails ?? throw new ArgumentNullException(nameof(updatedSowDetails));
    }

    public void ApproveBrief()
    {
        if (Status != ProjectStatus.Processed)
            throw new InvalidOperationException("Cannot approve brief. Expected Processed status.");
        if (SowDetails == null || !SowDetails.IsPopulated())
            throw new InvalidOperationException("Cannot approve brief. SOW Details are missing or incomplete.");

        var oldStatus = Status;
        Status = ProjectStatus.BriefApproved;
        AddDomainEvent(new BriefApprovedDomainEvent(Id));
        AddDomainEvent(new ProjectStatusChangedDomainEvent(Id, oldStatus.ToString(), Status.ToString()));
    }

    public void DistributeBrief()
    {
        if (Status != ProjectStatus.BriefApproved)
            throw new InvalidOperationException("Cannot distribute brief. Expected BriefApproved status.");

        var oldStatus = Status;
        Status = ProjectStatus.Proposed;
        AddDomainEvent(new ProjectStatusChangedDomainEvent(Id, oldStatus.ToString(), Status.ToString()));
    }

    public void AddProposal(Proposal proposal)
    {
        if (Status != ProjectStatus.Proposed)
            throw new InvalidOperationException("Cannot add proposal. Project is not in Proposed state.");
        if (_proposals.Any(p => p.VendorId == proposal.VendorId))
            throw new InvalidOperationException("This vendor has already submitted a proposal for this project.");
        _proposals.Add(proposal);
    }

    public void AwardTo(Guid proposalId)
    {
        if (Status != ProjectStatus.Proposed)
            throw new InvalidOperationException("Cannot award project. Expected Proposed status.");

        var winningProposal = _proposals.FirstOrDefault(p => p.Id == proposalId)
            ?? throw new ArgumentException("Proposal not found in this project.", nameof(proposalId));

        winningProposal.Accept();

        foreach (var proposal in _proposals.Where(p => p.Id != proposalId))
        {
            if (proposal.Status != ProposalStatus.Withdrawn)
                proposal.Reject();
        }

        AwardedVendorId = winningProposal.VendorId;
        AwardedProposalId = proposalId;
        var oldStatus = Status;
        Status = ProjectStatus.Awarded;

        AddDomainEvent(new ProjectAwardedDomainEvent(
            Id, winningProposal.VendorId, winningProposal.Id,
            winningProposal.ProposedCost, winningProposal.Currency));
        AddDomainEvent(new ProjectStatusChangedDomainEvent(Id, oldStatus.ToString(), Status.ToString()));
    }

    public void ConfigureFinancials(decimal? fixedMargin, decimal? percentageMargin)
    {
        if (Status is ProjectStatus.Active or ProjectStatus.Completed)
            throw new InvalidOperationException("Cannot modify financials for an active or completed project.");
        if (fixedMargin.HasValue && percentageMargin.HasValue)
            throw new ArgumentException("Cannot configure both Fixed Margin and Percentage Margin.");
        if (fixedMargin is < 0)
            throw new ArgumentException("Fixed margin cannot be negative.");
        if (percentageMargin is < 0 or > 100)
            throw new ArgumentException("Percentage margin must be between 0 and 100.");

        FixedMargin = fixedMargin;
        PercentageMargin = percentageMargin;
    }

    public void AddMilestone(Milestone milestone)
    {
        if (Status != ProjectStatus.Awarded && Status != ProjectStatus.Active)
            throw new InvalidOperationException("Cannot add milestones in current state.");
        _milestones.Add(milestone);
    }

    public void ApproveMilestone(Guid milestoneId, Guid contactId)
    {
        if (Status != ProjectStatus.Active)
            throw new InvalidOperationException("Cannot approve milestones when project is not Active.");

        var milestone = _milestones.FirstOrDefault(m => m.Id == milestoneId)
            ?? throw new ArgumentException("Milestone not found.", nameof(milestoneId));

        milestone.Approve(contactId);
        AddDomainEvent(new MilestoneApprovedDomainEvent(Id, milestoneId, milestone.Amount));
    }

    public void Activate()
    {
        if (Status != ProjectStatus.Awarded)
            throw new InvalidOperationException("Cannot activate project. Expected Awarded status.");

        var oldStatus = Status;
        Status = ProjectStatus.Active;
        AddDomainEvent(new ProjectStatusChangedDomainEvent(Id, oldStatus.ToString(), Status.ToString()));
    }

    public void Complete()
    {
        if (Status != ProjectStatus.Active)
            throw new InvalidOperationException("Cannot complete project. Expected Active status.");

        var oldStatus = Status;
        Status = ProjectStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        AddDomainEvent(new ProjectStatusChangedDomainEvent(Id, oldStatus.ToString(), Status.ToString()));
    }

    public void PutOnHold()
    {
        if (Status != ProjectStatus.Active)
            throw new InvalidOperationException("Cannot put on hold. Expected Active status.");

        var oldStatus = Status;
        Status = ProjectStatus.OnHold;
        AddDomainEvent(new ProjectStatusChangedDomainEvent(Id, oldStatus.ToString(), Status.ToString()));
    }

    public void Resume()
    {
        if (Status != ProjectStatus.OnHold)
            throw new InvalidOperationException("Cannot resume. Expected OnHold status.");

        var oldStatus = Status;
        Status = ProjectStatus.Active;
        AddDomainEvent(new ProjectStatusChangedDomainEvent(Id, oldStatus.ToString(), Status.ToString()));
    }

    public void Cancel()
    {
        if (Status is ProjectStatus.Completed or ProjectStatus.Cancelled)
            throw new InvalidOperationException("Cannot cancel project in current state.");

        var oldStatus = Status;
        Status = ProjectStatus.Cancelled;
        AddDomainEvent(new ProjectStatusChangedDomainEvent(Id, oldStatus.ToString(), Status.ToString()));
    }

    public void AddPayoutRule(ProjectPayoutRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        _payoutRules.Add(rule);
    }

    private void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
