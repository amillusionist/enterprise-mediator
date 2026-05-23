using EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;
using EnterpriseMediator.ProjectManagement.Domain.Enums;
using EnterpriseMediator.ProjectManagement.Domain.Events;
using FluentAssertions;
using Xunit;

namespace EnterpriseMediator.ProjectManagement.UnitTests.Domain;

public class ProjectAggregateTests
{
    private static readonly Guid ValidClientId = Guid.NewGuid();
    private static readonly Guid ValidUploaderId = Guid.NewGuid();
    private const string ValidProjectName = "Test Project";
    private const string ValidProjectDescription = "A test project description";

    #region Helpers

    private static Project CreatePendingProject()
    {
        return Project.Create(ValidClientId, ValidProjectName, ValidProjectDescription);
    }

    private static SowDetails CreatePopulatedSowDetails()
    {
        return new SowDetails(
            "Build a web application for client management.",
            new[] { "Web App", "Admin Dashboard" },
            new[] { "C#", ".NET", "React" },
            new[] { "PostgreSQL", "Docker" },
            "6 months");
    }

    private static SowDetails CreateEmptySowDetails()
    {
        return SowDetails.CreateEmpty();
    }

    private static Project CreateProcessingProject()
    {
        var project = CreatePendingProject();
        project.UploadSow("sow.pdf", "application/pdf", 1024, "storage/sow.pdf", ValidUploaderId);
        project.ClearDomainEvents();
        return project;
    }

    private static Project CreateProcessedProject()
    {
        var project = CreateProcessingProject();
        project.AttachSowDetails(CreatePopulatedSowDetails());
        project.ClearDomainEvents();
        return project;
    }

    private static Project CreateBriefApprovedProject()
    {
        var project = CreateProcessedProject();
        project.ApproveBrief();
        project.ClearDomainEvents();
        return project;
    }

    private static Project CreateProposedProject()
    {
        var project = CreateBriefApprovedProject();
        project.DistributeBrief();
        project.ClearDomainEvents();
        return project;
    }

    private static Proposal CreateProposal(Guid projectId, Guid? vendorId = null)
    {
        return new Proposal(
            projectId,
            vendorId ?? Guid.NewGuid(),
            50000m,
            "USD",
            "3 months",
            "John Doe, Jane Smith",
            "We are excited to submit this proposal.",
            "https://example.com/proposal.pdf");
    }

    private static Project CreateProposedProjectWithProposal(out Proposal proposal)
    {
        var project = CreateProposedProject();
        proposal = CreateProposal(project.Id);
        project.AddProposal(proposal);
        project.ClearDomainEvents();
        return project;
    }

    private static Project CreateAwardedProject()
    {
        var project = CreateProposedProjectWithProposal(out var proposal);
        project.AwardTo(proposal.Id);
        project.ClearDomainEvents();
        return project;
    }

    private static Project CreateActiveProject()
    {
        var project = CreateAwardedProject();
        project.Activate();
        project.ClearDomainEvents();
        return project;
    }

    private static Project CreateOnHoldProject()
    {
        var project = CreateActiveProject();
        project.PutOnHold();
        project.ClearDomainEvents();
        return project;
    }

    private static Project CreateCompletedProject()
    {
        var project = CreateActiveProject();
        project.Complete();
        project.ClearDomainEvents();
        return project;
    }

    private static Milestone CreateMilestone(Guid projectId, int order = 1)
    {
        return new Milestone(
            projectId,
            $"Milestone {order}",
            "Milestone description",
            10000m,
            "USD",
            order,
            DateTime.UtcNow.AddMonths(order));
    }

    #endregion

    #region Create() Factory

    [Fact]
    public void Create_WithValidParameters_ShouldReturnProjectInPendingStatus()
    {
        var project = Project.Create(ValidClientId, ValidProjectName, ValidProjectDescription);

        project.Should().NotBeNull();
        project.Id.Should().NotBe(Guid.Empty);
        project.ClientId.Should().Be(ValidClientId);
        project.Name.Should().Be(ValidProjectName);
        project.Description.Should().Be(ValidProjectDescription);
        project.Status.Should().Be(ProjectStatus.Pending);
        project.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        project.CompletedAt.Should().BeNull();
        project.SowDetails.Should().BeNull();
        project.SowDocument.Should().BeNull();
        project.FixedMargin.Should().BeNull();
        project.PercentageMargin.Should().BeNull();
        project.AwardedVendorId.Should().BeNull();
        project.AwardedProposalId.Should().BeNull();
        project.Proposals.Should().BeEmpty();
        project.Milestones.Should().BeEmpty();
        project.PayoutRules.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithValidParameters_ShouldRaiseProjectCreatedDomainEvent()
    {
        var project = Project.Create(ValidClientId, ValidProjectName, ValidProjectDescription);

        project.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProjectCreatedDomainEvent>()
            .Which.Should().Match<ProjectCreatedDomainEvent>(e =>
                e.ProjectId == project.Id &&
                e.ClientId == ValidClientId &&
                e.ProjectName == ValidProjectName);
    }

    [Fact]
    public void Create_WithNullDescription_ShouldSetDescriptionToEmptyString()
    {
        var project = Project.Create(ValidClientId, ValidProjectName, null!);

        project.Description.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithEmptyClientId_ShouldThrowArgumentException()
    {
        var act = () => Project.Create(Guid.Empty, ValidProjectName, ValidProjectDescription);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("clientId");
    }

    [Fact]
    public void Create_WithNullName_ShouldThrowArgumentException()
    {
        var act = () => Project.Create(ValidClientId, null!, ValidProjectDescription);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowArgumentException()
    {
        var act = () => Project.Create(ValidClientId, "", ValidProjectDescription);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrowArgumentException()
    {
        var act = () => Project.Create(ValidClientId, "   ", ValidProjectDescription);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("name");
    }

    #endregion

    #region UploadSow()

    [Fact]
    public void UploadSow_WhenPending_ShouldSetSowDocumentAndTransitionToProcessing()
    {
        var project = CreatePendingProject();
        project.ClearDomainEvents();

        var sow = project.UploadSow("sow.pdf", "application/pdf", 2048, "storage/key", ValidUploaderId);

        sow.Should().NotBeNull();
        sow.FileName.Should().Be("sow.pdf");
        sow.ContentType.Should().Be("application/pdf");
        sow.FileSizeBytes.Should().Be(2048);
        sow.StorageKey.Should().Be("storage/key");
        sow.UploadedBy.Should().Be(ValidUploaderId);
        project.SowDocument.Should().BeSameAs(sow);
        project.Status.Should().Be(ProjectStatus.Processing);
    }

    [Fact]
    public void UploadSow_WhenPending_ShouldRaiseSowUploadedAndStatusChangedEvents()
    {
        var project = CreatePendingProject();
        project.ClearDomainEvents();

        project.UploadSow("sow.pdf", "application/pdf", 2048, "storage/key", ValidUploaderId);

        project.DomainEvents.Should().HaveCount(2);
        project.DomainEvents.Should().ContainSingle(e => e is SowUploadedDomainEvent);
        project.DomainEvents.Should().ContainSingle(e => e is ProjectStatusChangedDomainEvent);

        var statusEvent = project.DomainEvents.OfType<ProjectStatusChangedDomainEvent>().Single();
        statusEvent.OldStatus.Should().Be(ProjectStatus.Pending.ToString());
        statusEvent.NewStatus.Should().Be(ProjectStatus.Processing.ToString());
    }

    [Fact]
    public void UploadSow_WhenFailed_ShouldAllowReupload()
    {
        var project = CreateProcessingProject();
        project.MarkSowFailed("Parse error");
        project.ClearDomainEvents();

        var act = () => project.UploadSow("sow_v2.pdf", "application/pdf", 4096, "storage/key2", ValidUploaderId);

        act.Should().NotThrow();
        project.Status.Should().Be(ProjectStatus.Processing);
        project.SowDocument!.FileName.Should().Be("sow_v2.pdf");
    }

    [Theory]
    [InlineData(ProjectStatus.Processing)]
    [InlineData(ProjectStatus.Processed)]
    [InlineData(ProjectStatus.BriefApproved)]
    [InlineData(ProjectStatus.Proposed)]
    [InlineData(ProjectStatus.Awarded)]
    [InlineData(ProjectStatus.Active)]
    [InlineData(ProjectStatus.Completed)]
    [InlineData(ProjectStatus.OnHold)]
    [InlineData(ProjectStatus.Cancelled)]
    public void UploadSow_WhenInInvalidStatus_ShouldThrowInvalidOperationException(ProjectStatus invalidStatus)
    {
        var project = GetProjectInStatus(invalidStatus);

        var act = () => project.UploadSow("sow.pdf", "application/pdf", 2048, "storage/key", ValidUploaderId);

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region AttachSowDetails()

    [Fact]
    public void AttachSowDetails_WhenProcessing_ShouldSetSowDetailsAndTransitionToProcessed()
    {
        var project = CreateProcessingProject();
        var sowDetails = CreatePopulatedSowDetails();

        project.AttachSowDetails(sowDetails);

        project.SowDetails.Should().BeSameAs(sowDetails);
        project.Status.Should().Be(ProjectStatus.Processed);
    }

    [Fact]
    public void AttachSowDetails_WhenProcessing_ShouldRaiseStatusChangedEvent()
    {
        var project = CreateProcessingProject();

        project.AttachSowDetails(CreatePopulatedSowDetails());

        project.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProjectStatusChangedDomainEvent>()
            .Which.Should().Match<ProjectStatusChangedDomainEvent>(e =>
                e.OldStatus == ProjectStatus.Processing.ToString() &&
                e.NewStatus == ProjectStatus.Processed.ToString());
    }

    [Fact]
    public void AttachSowDetails_WithNull_ShouldThrowArgumentNullException()
    {
        var project = CreateProcessingProject();

        var act = () => project.AttachSowDetails(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(ProjectStatus.Pending)]
    [InlineData(ProjectStatus.Processed)]
    [InlineData(ProjectStatus.BriefApproved)]
    [InlineData(ProjectStatus.Proposed)]
    [InlineData(ProjectStatus.Awarded)]
    [InlineData(ProjectStatus.Active)]
    [InlineData(ProjectStatus.Completed)]
    public void AttachSowDetails_WhenNotProcessing_ShouldThrowInvalidOperationException(ProjectStatus invalidStatus)
    {
        var project = GetProjectInStatus(invalidStatus);

        var act = () => project.AttachSowDetails(CreatePopulatedSowDetails());

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region MarkSowFailed()

    [Fact]
    public void MarkSowFailed_WhenProcessing_ShouldTransitionToFailed()
    {
        var project = CreateProcessingProject();

        project.MarkSowFailed("Parsing failed: corrupted PDF");

        project.Status.Should().Be(ProjectStatus.Failed);
    }

    [Fact]
    public void MarkSowFailed_WhenProcessing_ShouldRaiseStatusChangedEvent()
    {
        var project = CreateProcessingProject();

        project.MarkSowFailed("Error");

        project.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProjectStatusChangedDomainEvent>()
            .Which.Should().Match<ProjectStatusChangedDomainEvent>(e =>
                e.OldStatus == ProjectStatus.Processing.ToString() &&
                e.NewStatus == ProjectStatus.Failed.ToString());
    }

    [Theory]
    [InlineData(ProjectStatus.Pending)]
    [InlineData(ProjectStatus.Processed)]
    [InlineData(ProjectStatus.BriefApproved)]
    [InlineData(ProjectStatus.Active)]
    public void MarkSowFailed_WhenNotProcessing_ShouldThrowInvalidOperationException(ProjectStatus invalidStatus)
    {
        var project = GetProjectInStatus(invalidStatus);

        var act = () => project.MarkSowFailed("Error");

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region UpdateBrief()

    [Fact]
    public void UpdateBrief_WhenProcessed_ShouldUpdateSowDetails()
    {
        var project = CreateProcessedProject();
        var updatedDetails = new SowDetails(
            "Updated scope summary",
            new[] { "Updated Deliverable" },
            new[] { "Python" },
            new[] { "AWS" },
            "12 months");

        project.UpdateBrief(updatedDetails);

        project.SowDetails.Should().BeSameAs(updatedDetails);
        project.SowDetails!.ScopeSummary.Should().Be("Updated scope summary");
    }

    [Fact]
    public void UpdateBrief_WhenBriefApproved_ShouldUpdateSowDetails()
    {
        var project = CreateBriefApprovedProject();
        var updatedDetails = CreatePopulatedSowDetails();

        var act = () => project.UpdateBrief(updatedDetails);

        act.Should().NotThrow();
        project.SowDetails.Should().BeSameAs(updatedDetails);
    }

    [Fact]
    public void UpdateBrief_WithNull_ShouldThrowArgumentNullException()
    {
        var project = CreateProcessedProject();

        var act = () => project.UpdateBrief(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(ProjectStatus.Pending)]
    [InlineData(ProjectStatus.Processing)]
    [InlineData(ProjectStatus.Proposed)]
    [InlineData(ProjectStatus.Awarded)]
    [InlineData(ProjectStatus.Active)]
    [InlineData(ProjectStatus.Completed)]
    public void UpdateBrief_WhenInInvalidStatus_ShouldThrowInvalidOperationException(ProjectStatus invalidStatus)
    {
        var project = GetProjectInStatus(invalidStatus);

        var act = () => project.UpdateBrief(CreatePopulatedSowDetails());

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region ApproveBrief()

    [Fact]
    public void ApproveBrief_WhenProcessedWithPopulatedSowDetails_ShouldTransitionToBriefApproved()
    {
        var project = CreateProcessedProject();

        project.ApproveBrief();

        project.Status.Should().Be(ProjectStatus.BriefApproved);
    }

    [Fact]
    public void ApproveBrief_WhenProcessed_ShouldRaiseBriefApprovedAndStatusChangedEvents()
    {
        var project = CreateProcessedProject();

        project.ApproveBrief();

        project.DomainEvents.Should().HaveCount(2);
        project.DomainEvents.Should().ContainSingle(e => e is BriefApprovedDomainEvent)
            .Which.Should().BeOfType<BriefApprovedDomainEvent>()
            .Which.ProjectId.Should().Be(project.Id);
        project.DomainEvents.Should().ContainSingle(e => e is ProjectStatusChangedDomainEvent);
    }

    [Fact]
    public void ApproveBrief_WhenSowDetailsAreEmpty_ShouldThrowInvalidOperationException()
    {
        var project = CreateProcessingProject();
        project.AttachSowDetails(CreateEmptySowDetails());
        project.ClearDomainEvents();

        var act = () => project.ApproveBrief();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*SOW Details are missing or incomplete*");
    }

    [Theory]
    [InlineData(ProjectStatus.Pending)]
    [InlineData(ProjectStatus.Processing)]
    [InlineData(ProjectStatus.BriefApproved)]
    [InlineData(ProjectStatus.Proposed)]
    [InlineData(ProjectStatus.Awarded)]
    [InlineData(ProjectStatus.Active)]
    [InlineData(ProjectStatus.Completed)]
    public void ApproveBrief_WhenNotProcessed_ShouldThrowInvalidOperationException(ProjectStatus invalidStatus)
    {
        var project = GetProjectInStatus(invalidStatus);

        var act = () => project.ApproveBrief();

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region DistributeBrief()

    [Fact]
    public void DistributeBrief_WhenBriefApproved_ShouldTransitionToProposed()
    {
        var project = CreateBriefApprovedProject();

        project.DistributeBrief();

        project.Status.Should().Be(ProjectStatus.Proposed);
    }

    [Fact]
    public void DistributeBrief_WhenBriefApproved_ShouldRaiseStatusChangedEvent()
    {
        var project = CreateBriefApprovedProject();

        project.DistributeBrief();

        project.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProjectStatusChangedDomainEvent>()
            .Which.Should().Match<ProjectStatusChangedDomainEvent>(e =>
                e.OldStatus == ProjectStatus.BriefApproved.ToString() &&
                e.NewStatus == ProjectStatus.Proposed.ToString());
    }

    [Theory]
    [InlineData(ProjectStatus.Pending)]
    [InlineData(ProjectStatus.Processing)]
    [InlineData(ProjectStatus.Processed)]
    [InlineData(ProjectStatus.Proposed)]
    [InlineData(ProjectStatus.Awarded)]
    [InlineData(ProjectStatus.Active)]
    [InlineData(ProjectStatus.Completed)]
    public void DistributeBrief_WhenNotBriefApproved_ShouldThrowInvalidOperationException(ProjectStatus invalidStatus)
    {
        var project = GetProjectInStatus(invalidStatus);

        var act = () => project.DistributeBrief();

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region AddProposal()

    [Fact]
    public void AddProposal_WhenProposed_ShouldAddProposalToCollection()
    {
        var project = CreateProposedProject();
        var proposal = CreateProposal(project.Id);

        project.AddProposal(proposal);

        project.Proposals.Should().ContainSingle()
            .Which.Should().BeSameAs(proposal);
    }

    [Fact]
    public void AddProposal_WhenProposed_ShouldAllowMultipleProposalsFromDifferentVendors()
    {
        var project = CreateProposedProject();
        var proposal1 = CreateProposal(project.Id, Guid.NewGuid());
        var proposal2 = CreateProposal(project.Id, Guid.NewGuid());

        project.AddProposal(proposal1);
        project.AddProposal(proposal2);

        project.Proposals.Should().HaveCount(2);
    }

    [Fact]
    public void AddProposal_DuplicateVendor_ShouldThrowInvalidOperationException()
    {
        var project = CreateProposedProject();
        var vendorId = Guid.NewGuid();
        var proposal1 = CreateProposal(project.Id, vendorId);
        var proposal2 = CreateProposal(project.Id, vendorId);

        project.AddProposal(proposal1);
        var act = () => project.AddProposal(proposal2);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already submitted*");
    }

    [Theory]
    [InlineData(ProjectStatus.Pending)]
    [InlineData(ProjectStatus.Processing)]
    [InlineData(ProjectStatus.Processed)]
    [InlineData(ProjectStatus.BriefApproved)]
    [InlineData(ProjectStatus.Awarded)]
    [InlineData(ProjectStatus.Active)]
    [InlineData(ProjectStatus.Completed)]
    public void AddProposal_WhenNotProposed_ShouldThrowInvalidOperationException(ProjectStatus invalidStatus)
    {
        var project = GetProjectInStatus(invalidStatus);
        var proposal = CreateProposal(project.Id);

        var act = () => project.AddProposal(proposal);

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region AwardTo()

    [Fact]
    public void AwardTo_WhenProposedWithValidProposal_ShouldTransitionToAwarded()
    {
        var project = CreateProposedProjectWithProposal(out var proposal);

        project.AwardTo(proposal.Id);

        project.Status.Should().Be(ProjectStatus.Awarded);
        project.AwardedVendorId.Should().Be(proposal.VendorId);
        project.AwardedProposalId.Should().Be(proposal.Id);
    }

    [Fact]
    public void AwardTo_ShouldAcceptWinningAndRejectOtherProposals()
    {
        var project = CreateProposedProject();
        var winningProposal = CreateProposal(project.Id, Guid.NewGuid());
        var losingProposal = CreateProposal(project.Id, Guid.NewGuid());
        project.AddProposal(winningProposal);
        project.AddProposal(losingProposal);
        project.ClearDomainEvents();

        project.AwardTo(winningProposal.Id);

        winningProposal.Status.Should().Be(ProposalStatus.Accepted);
        losingProposal.Status.Should().Be(ProposalStatus.Rejected);
    }

    [Fact]
    public void AwardTo_ShouldNotRejectWithdrawnProposals()
    {
        var project = CreateProposedProject();
        var winningProposal = CreateProposal(project.Id, Guid.NewGuid());
        var withdrawnProposal = CreateProposal(project.Id, Guid.NewGuid());
        project.AddProposal(winningProposal);
        project.AddProposal(withdrawnProposal);
        withdrawnProposal.Withdraw();
        project.ClearDomainEvents();

        project.AwardTo(winningProposal.Id);

        withdrawnProposal.Status.Should().Be(ProposalStatus.Withdrawn);
    }

    [Fact]
    public void AwardTo_ShouldRaiseProjectAwardedAndStatusChangedEvents()
    {
        var project = CreateProposedProjectWithProposal(out var proposal);

        project.AwardTo(proposal.Id);

        project.DomainEvents.Should().HaveCount(2);

        var awardedEvent = project.DomainEvents.OfType<ProjectAwardedDomainEvent>().Single();
        awardedEvent.ProjectId.Should().Be(project.Id);
        awardedEvent.VendorId.Should().Be(proposal.VendorId);
        awardedEvent.ProposalId.Should().Be(proposal.Id);
        awardedEvent.AwardedAmount.Should().Be(proposal.ProposedCost);
        awardedEvent.Currency.Should().Be(proposal.Currency);

        project.DomainEvents.Should().ContainSingle(e => e is ProjectStatusChangedDomainEvent);
    }

    [Fact]
    public void AwardTo_WithInvalidProposalId_ShouldThrowArgumentException()
    {
        var project = CreateProposedProjectWithProposal(out _);

        var act = () => project.AwardTo(Guid.NewGuid());

        act.Should().Throw<ArgumentException>()
            .WithParameterName("proposalId")
            .WithMessage("*Proposal not found*");
    }

    [Theory]
    [InlineData(ProjectStatus.Pending)]
    [InlineData(ProjectStatus.Processing)]
    [InlineData(ProjectStatus.Processed)]
    [InlineData(ProjectStatus.BriefApproved)]
    [InlineData(ProjectStatus.Awarded)]
    [InlineData(ProjectStatus.Active)]
    [InlineData(ProjectStatus.Completed)]
    public void AwardTo_WhenNotProposed_ShouldThrowInvalidOperationException(ProjectStatus invalidStatus)
    {
        var project = GetProjectInStatus(invalidStatus);

        var act = () => project.AwardTo(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region ConfigureFinancials()

    [Fact]
    public void ConfigureFinancials_WithFixedMarginOnly_ShouldSetFixedMargin()
    {
        var project = CreateAwardedProject();

        project.ConfigureFinancials(5000m, null);

        project.FixedMargin.Should().Be(5000m);
        project.PercentageMargin.Should().BeNull();
    }

    [Fact]
    public void ConfigureFinancials_WithPercentageMarginOnly_ShouldSetPercentageMargin()
    {
        var project = CreateAwardedProject();

        project.ConfigureFinancials(null, 15m);

        project.FixedMargin.Should().BeNull();
        project.PercentageMargin.Should().Be(15m);
    }

    [Fact]
    public void ConfigureFinancials_WithBothNull_ShouldClearMargins()
    {
        var project = CreateAwardedProject();
        project.ConfigureFinancials(5000m, null);

        project.ConfigureFinancials(null, null);

        project.FixedMargin.Should().BeNull();
        project.PercentageMargin.Should().BeNull();
    }

    [Fact]
    public void ConfigureFinancials_WhenPending_ShouldSucceed()
    {
        var project = CreatePendingProject();

        var act = () => project.ConfigureFinancials(1000m, null);

        act.Should().NotThrow();
    }

    [Fact]
    public void ConfigureFinancials_WithBothMargins_ShouldThrowArgumentException()
    {
        var project = CreateAwardedProject();

        var act = () => project.ConfigureFinancials(5000m, 15m);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Cannot configure both*");
    }

    [Fact]
    public void ConfigureFinancials_WithNegativeFixedMargin_ShouldThrowArgumentException()
    {
        var project = CreateAwardedProject();

        var act = () => project.ConfigureFinancials(-100m, null);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Fixed margin cannot be negative*");
    }

    [Fact]
    public void ConfigureFinancials_WithNegativePercentageMargin_ShouldThrowArgumentException()
    {
        var project = CreateAwardedProject();

        var act = () => project.ConfigureFinancials(null, -5m);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Percentage margin must be between 0 and 100*");
    }

    [Fact]
    public void ConfigureFinancials_WithPercentageMarginOver100_ShouldThrowArgumentException()
    {
        var project = CreateAwardedProject();

        var act = () => project.ConfigureFinancials(null, 101m);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Percentage margin must be between 0 and 100*");
    }

    [Fact]
    public void ConfigureFinancials_WithZeroFixedMargin_ShouldSucceed()
    {
        var project = CreateAwardedProject();

        var act = () => project.ConfigureFinancials(0m, null);

        act.Should().NotThrow();
        project.FixedMargin.Should().Be(0m);
    }

    [Fact]
    public void ConfigureFinancials_WithPercentageMarginAt100_ShouldSucceed()
    {
        var project = CreateAwardedProject();

        var act = () => project.ConfigureFinancials(null, 100m);

        act.Should().NotThrow();
        project.PercentageMargin.Should().Be(100m);
    }

    [Fact]
    public void ConfigureFinancials_WhenActive_ShouldThrowInvalidOperationException()
    {
        var project = CreateActiveProject();

        var act = () => project.ConfigureFinancials(5000m, null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot modify financials*");
    }

    [Fact]
    public void ConfigureFinancials_WhenCompleted_ShouldThrowInvalidOperationException()
    {
        var project = CreateCompletedProject();

        var act = () => project.ConfigureFinancials(5000m, null);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot modify financials*");
    }

    #endregion

    #region AddMilestone()

    [Fact]
    public void AddMilestone_WhenAwarded_ShouldAddMilestoneToCollection()
    {
        var project = CreateAwardedProject();
        var milestone = CreateMilestone(project.Id);

        project.AddMilestone(milestone);

        project.Milestones.Should().ContainSingle()
            .Which.Should().BeSameAs(milestone);
    }

    [Fact]
    public void AddMilestone_WhenActive_ShouldAddMilestoneToCollection()
    {
        var project = CreateActiveProject();
        var milestone = CreateMilestone(project.Id);

        project.AddMilestone(milestone);

        project.Milestones.Should().ContainSingle();
    }

    [Fact]
    public void AddMilestone_ShouldAllowMultipleMilestones()
    {
        var project = CreateAwardedProject();
        var milestone1 = CreateMilestone(project.Id, 1);
        var milestone2 = CreateMilestone(project.Id, 2);

        project.AddMilestone(milestone1);
        project.AddMilestone(milestone2);

        project.Milestones.Should().HaveCount(2);
    }

    [Theory]
    [InlineData(ProjectStatus.Pending)]
    [InlineData(ProjectStatus.Processing)]
    [InlineData(ProjectStatus.Processed)]
    [InlineData(ProjectStatus.BriefApproved)]
    [InlineData(ProjectStatus.Proposed)]
    [InlineData(ProjectStatus.Completed)]
    [InlineData(ProjectStatus.OnHold)]
    [InlineData(ProjectStatus.Cancelled)]
    public void AddMilestone_WhenInInvalidStatus_ShouldThrowInvalidOperationException(ProjectStatus invalidStatus)
    {
        var project = GetProjectInStatus(invalidStatus);
        var milestone = CreateMilestone(project.Id);

        var act = () => project.AddMilestone(milestone);

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region ApproveMilestone()

    [Fact]
    public void ApproveMilestone_WhenActiveWithValidMilestone_ShouldApproveMilestone()
    {
        var project = CreateActiveProject();
        var milestone = CreateMilestone(project.Id);
        project.AddMilestone(milestone);
        milestone.MarkInProgress();
        milestone.SubmitForApproval();
        project.ClearDomainEvents();
        var contactId = Guid.NewGuid();

        project.ApproveMilestone(milestone.Id, contactId);

        milestone.Status.Should().Be(MilestoneStatus.Approved);
        milestone.ApprovedByContactId.Should().Be(contactId);
        milestone.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public void ApproveMilestone_WhenActive_ShouldRaiseMilestoneApprovedDomainEvent()
    {
        var project = CreateActiveProject();
        var milestone = CreateMilestone(project.Id);
        project.AddMilestone(milestone);
        milestone.MarkInProgress();
        milestone.SubmitForApproval();
        project.ClearDomainEvents();
        var contactId = Guid.NewGuid();

        project.ApproveMilestone(milestone.Id, contactId);

        project.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<MilestoneApprovedDomainEvent>()
            .Which.Should().Match<MilestoneApprovedDomainEvent>(e =>
                e.ProjectId == project.Id &&
                e.MilestoneId == milestone.Id &&
                e.PayoutAmount == milestone.Amount);
    }

    [Fact]
    public void ApproveMilestone_WithInvalidMilestoneId_ShouldThrowArgumentException()
    {
        var project = CreateActiveProject();
        var contactId = Guid.NewGuid();

        var act = () => project.ApproveMilestone(Guid.NewGuid(), contactId);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("milestoneId")
            .WithMessage("*Milestone not found*");
    }

    [Theory]
    [InlineData(ProjectStatus.Pending)]
    [InlineData(ProjectStatus.Processing)]
    [InlineData(ProjectStatus.Processed)]
    [InlineData(ProjectStatus.BriefApproved)]
    [InlineData(ProjectStatus.Proposed)]
    [InlineData(ProjectStatus.Awarded)]
    [InlineData(ProjectStatus.Completed)]
    [InlineData(ProjectStatus.OnHold)]
    [InlineData(ProjectStatus.Cancelled)]
    public void ApproveMilestone_WhenNotActive_ShouldThrowInvalidOperationException(ProjectStatus invalidStatus)
    {
        var project = GetProjectInStatus(invalidStatus);

        var act = () => project.ApproveMilestone(Guid.NewGuid(), Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Activate()

    [Fact]
    public void Activate_WhenAwarded_ShouldTransitionToActive()
    {
        var project = CreateAwardedProject();

        project.Activate();

        project.Status.Should().Be(ProjectStatus.Active);
    }

    [Fact]
    public void Activate_WhenAwarded_ShouldRaiseStatusChangedEvent()
    {
        var project = CreateAwardedProject();

        project.Activate();

        project.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProjectStatusChangedDomainEvent>()
            .Which.Should().Match<ProjectStatusChangedDomainEvent>(e =>
                e.OldStatus == ProjectStatus.Awarded.ToString() &&
                e.NewStatus == ProjectStatus.Active.ToString());
    }

    [Theory]
    [InlineData(ProjectStatus.Pending)]
    [InlineData(ProjectStatus.Processing)]
    [InlineData(ProjectStatus.Processed)]
    [InlineData(ProjectStatus.BriefApproved)]
    [InlineData(ProjectStatus.Proposed)]
    [InlineData(ProjectStatus.Active)]
    [InlineData(ProjectStatus.Completed)]
    [InlineData(ProjectStatus.OnHold)]
    [InlineData(ProjectStatus.Cancelled)]
    public void Activate_WhenNotAwarded_ShouldThrowInvalidOperationException(ProjectStatus invalidStatus)
    {
        var project = GetProjectInStatus(invalidStatus);

        var act = () => project.Activate();

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Complete()

    [Fact]
    public void Complete_WhenActive_ShouldTransitionToCompleted()
    {
        var project = CreateActiveProject();

        project.Complete();

        project.Status.Should().Be(ProjectStatus.Completed);
        project.CompletedAt.Should().NotBeNull();
        project.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Complete_WhenActive_ShouldRaiseStatusChangedEvent()
    {
        var project = CreateActiveProject();

        project.Complete();

        project.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProjectStatusChangedDomainEvent>()
            .Which.Should().Match<ProjectStatusChangedDomainEvent>(e =>
                e.OldStatus == ProjectStatus.Active.ToString() &&
                e.NewStatus == ProjectStatus.Completed.ToString());
    }

    [Theory]
    [InlineData(ProjectStatus.Pending)]
    [InlineData(ProjectStatus.Processing)]
    [InlineData(ProjectStatus.Processed)]
    [InlineData(ProjectStatus.BriefApproved)]
    [InlineData(ProjectStatus.Proposed)]
    [InlineData(ProjectStatus.Awarded)]
    [InlineData(ProjectStatus.Completed)]
    [InlineData(ProjectStatus.OnHold)]
    [InlineData(ProjectStatus.Cancelled)]
    public void Complete_WhenNotActive_ShouldThrowInvalidOperationException(ProjectStatus invalidStatus)
    {
        var project = GetProjectInStatus(invalidStatus);

        var act = () => project.Complete();

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region PutOnHold()

    [Fact]
    public void PutOnHold_WhenActive_ShouldTransitionToOnHold()
    {
        var project = CreateActiveProject();

        project.PutOnHold();

        project.Status.Should().Be(ProjectStatus.OnHold);
    }

    [Fact]
    public void PutOnHold_WhenActive_ShouldRaiseStatusChangedEvent()
    {
        var project = CreateActiveProject();

        project.PutOnHold();

        project.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProjectStatusChangedDomainEvent>()
            .Which.Should().Match<ProjectStatusChangedDomainEvent>(e =>
                e.OldStatus == ProjectStatus.Active.ToString() &&
                e.NewStatus == ProjectStatus.OnHold.ToString());
    }

    [Theory]
    [InlineData(ProjectStatus.Pending)]
    [InlineData(ProjectStatus.Processing)]
    [InlineData(ProjectStatus.Processed)]
    [InlineData(ProjectStatus.BriefApproved)]
    [InlineData(ProjectStatus.Proposed)]
    [InlineData(ProjectStatus.Awarded)]
    [InlineData(ProjectStatus.Completed)]
    [InlineData(ProjectStatus.OnHold)]
    [InlineData(ProjectStatus.Cancelled)]
    public void PutOnHold_WhenNotActive_ShouldThrowInvalidOperationException(ProjectStatus invalidStatus)
    {
        var project = GetProjectInStatus(invalidStatus);

        var act = () => project.PutOnHold();

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Resume()

    [Fact]
    public void Resume_WhenOnHold_ShouldTransitionToActive()
    {
        var project = CreateOnHoldProject();

        project.Resume();

        project.Status.Should().Be(ProjectStatus.Active);
    }

    [Fact]
    public void Resume_WhenOnHold_ShouldRaiseStatusChangedEvent()
    {
        var project = CreateOnHoldProject();

        project.Resume();

        project.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProjectStatusChangedDomainEvent>()
            .Which.Should().Match<ProjectStatusChangedDomainEvent>(e =>
                e.OldStatus == ProjectStatus.OnHold.ToString() &&
                e.NewStatus == ProjectStatus.Active.ToString());
    }

    [Theory]
    [InlineData(ProjectStatus.Pending)]
    [InlineData(ProjectStatus.Processing)]
    [InlineData(ProjectStatus.Processed)]
    [InlineData(ProjectStatus.BriefApproved)]
    [InlineData(ProjectStatus.Proposed)]
    [InlineData(ProjectStatus.Awarded)]
    [InlineData(ProjectStatus.Active)]
    [InlineData(ProjectStatus.Completed)]
    [InlineData(ProjectStatus.Cancelled)]
    public void Resume_WhenNotOnHold_ShouldThrowInvalidOperationException(ProjectStatus invalidStatus)
    {
        var project = GetProjectInStatus(invalidStatus);

        var act = () => project.Resume();

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Cancel()

    [Fact]
    public void Cancel_WhenPending_ShouldTransitionToCancelled()
    {
        var project = CreatePendingProject();
        project.ClearDomainEvents();

        project.Cancel();

        project.Status.Should().Be(ProjectStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenActive_ShouldTransitionToCancelled()
    {
        var project = CreateActiveProject();

        project.Cancel();

        project.Status.Should().Be(ProjectStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenOnHold_ShouldTransitionToCancelled()
    {
        var project = CreateOnHoldProject();

        project.Cancel();

        project.Status.Should().Be(ProjectStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAwarded_ShouldTransitionToCancelled()
    {
        var project = CreateAwardedProject();

        project.Cancel();

        project.Status.Should().Be(ProjectStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenProposed_ShouldTransitionToCancelled()
    {
        var project = CreateProposedProject();

        project.Cancel();

        project.Status.Should().Be(ProjectStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ShouldRaiseStatusChangedEvent()
    {
        var project = CreateActiveProject();

        project.Cancel();

        project.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ProjectStatusChangedDomainEvent>()
            .Which.Should().Match<ProjectStatusChangedDomainEvent>(e =>
                e.OldStatus == ProjectStatus.Active.ToString() &&
                e.NewStatus == ProjectStatus.Cancelled.ToString());
    }

    [Fact]
    public void Cancel_WhenCompleted_ShouldThrowInvalidOperationException()
    {
        var project = CreateCompletedProject();

        var act = () => project.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrowInvalidOperationException()
    {
        var project = CreateActiveProject();
        project.Cancel();
        project.ClearDomainEvents();

        var act = () => project.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region ClearDomainEvents()

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        var project = CreatePendingProject();
        project.DomainEvents.Should().NotBeEmpty();

        project.ClearDomainEvents();

        project.DomainEvents.Should().BeEmpty();
    }

    #endregion

    #region AddPayoutRule()

    [Fact]
    public void AddPayoutRule_WithValidRule_ShouldAddToCollection()
    {
        var project = CreateAwardedProject();
        var rule = new ProjectPayoutRule(project.Id, "Kickoff", 50m, 1);

        project.AddPayoutRule(rule);

        project.PayoutRules.Should().ContainSingle()
            .Which.Should().BeSameAs(rule);
    }

    [Fact]
    public void AddPayoutRule_WithNull_ShouldThrowArgumentNullException()
    {
        var project = CreateAwardedProject();

        var act = () => project.AddPayoutRule(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Full Lifecycle Integration

    [Fact]
    public void FullLifecycle_PendingThroughCompleted_ShouldTraverseAllStatesCorrectly()
    {
        // Create
        var project = Project.Create(ValidClientId, ValidProjectName, ValidProjectDescription);
        project.Status.Should().Be(ProjectStatus.Pending);

        // Upload SOW
        project.UploadSow("sow.pdf", "application/pdf", 1024, "s3/sow.pdf", ValidUploaderId);
        project.Status.Should().Be(ProjectStatus.Processing);

        // Attach SOW Details
        project.AttachSowDetails(CreatePopulatedSowDetails());
        project.Status.Should().Be(ProjectStatus.Processed);

        // Approve Brief
        project.ApproveBrief();
        project.Status.Should().Be(ProjectStatus.BriefApproved);

        // Distribute Brief
        project.DistributeBrief();
        project.Status.Should().Be(ProjectStatus.Proposed);

        // Add Proposal and Award
        var proposal = CreateProposal(project.Id);
        project.AddProposal(proposal);
        project.AwardTo(proposal.Id);
        project.Status.Should().Be(ProjectStatus.Awarded);

        // Activate
        project.Activate();
        project.Status.Should().Be(ProjectStatus.Active);

        // Put on hold and resume
        project.PutOnHold();
        project.Status.Should().Be(ProjectStatus.OnHold);
        project.Resume();
        project.Status.Should().Be(ProjectStatus.Active);

        // Complete
        project.Complete();
        project.Status.Should().Be(ProjectStatus.Completed);
        project.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void FullLifecycle_PendingThroughCancelled_ShouldCancelFromActiveState()
    {
        var project = CreateActiveProject();

        project.Cancel();

        project.Status.Should().Be(ProjectStatus.Cancelled);
    }

    #endregion

    #region Status Helper

    /// <summary>
    /// Creates a project already advanced to the requested status. Used by Theory tests
    /// to verify guard clauses on methods that require specific statuses.
    /// Note: Failed and Disputed statuses are handled specially.
    /// </summary>
    private static Project GetProjectInStatus(ProjectStatus status)
    {
        return status switch
        {
            ProjectStatus.Pending => CreatePendingProject(),
            ProjectStatus.Processing => CreateProcessingProject(),
            ProjectStatus.Failed => CreateFailedProject(),
            ProjectStatus.Processed => CreateProcessedProject(),
            ProjectStatus.BriefApproved => CreateBriefApprovedProject(),
            ProjectStatus.Proposed => CreateProposedProject(),
            ProjectStatus.Awarded => CreateAwardedProject(),
            ProjectStatus.Active => CreateActiveProject(),
            ProjectStatus.Completed => CreateCompletedProject(),
            ProjectStatus.OnHold => CreateOnHoldProject(),
            ProjectStatus.Cancelled => CreateCancelledProject(),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported test status")
        };
    }

    private static Project CreateFailedProject()
    {
        var project = CreateProcessingProject();
        project.MarkSowFailed("Test failure");
        project.ClearDomainEvents();
        return project;
    }

    private static Project CreateCancelledProject()
    {
        var project = CreateActiveProject();
        project.Cancel();
        project.ClearDomainEvents();
        return project;
    }

    #endregion
}
