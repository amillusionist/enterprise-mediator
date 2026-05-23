namespace EnterpriseMediator.ProjectManagement.Domain.Enums;

public enum ProjectStatus
{
    Pending = 0,
    Processing = 1,
    Failed = 2,
    Processed = 3,
    BriefApproved = 4,
    Proposed = 5,
    Awarded = 6,
    Active = 7,
    Completed = 8,
    OnHold = 9,
    Cancelled = 10,
    Disputed = 11
}

public enum ProposalStatus
{
    Submitted = 0,
    InReview = 1,
    Shortlisted = 2,
    Accepted = 3,
    Rejected = 4,
    Withdrawn = 5
}

public enum MilestoneStatus
{
    Pending = 0,
    InProgress = 1,
    PendingApproval = 2,
    Approved = 3,
    Rejected = 4
}

public enum SowDocumentStatus
{
    Pending = 0,
    Processing = 1,
    Processed = 2,
    Failed = 3
}
