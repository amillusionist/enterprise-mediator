using System.Text.Json;
using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Projects;
using EnterpriseMediator.Contracts.Enums;
using EnterpriseMediator.Contracts.Events.Financials;
using EnterpriseMediator.Contracts.Events.Projects;
using EnterpriseMediator.Contracts.Events.Users;

namespace EnterpriseMediator.Contracts.Tests;

public class IntegrationEventTests
{
    [Fact]
    public void SowUploadedEvent_Implements_IIntegrationEvent()
    {
        var evt = new SowUploadedEvent
        {
            EventId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            SowId = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            S3ObjectKey = "sow/2024/project-123.pdf"
        };

        evt.Should().BeAssignableTo<IIntegrationEvent>();
        evt.S3ObjectKey.Should().Be("sow/2024/project-123.pdf");
    }

    [Fact]
    public void ProjectBriefApprovedEvent_Carries_BriefData()
    {
        var brief = new ProjectBriefDto
        {
            Id = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            Title = "Cloud Migration",
            RequiredSkills = new[] { "AWS", "Terraform", "Docker" },
            IsApproved = true
        };

        var evt = new ProjectBriefApprovedEvent
        {
            EventId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            ProjectId = brief.ProjectId,
            Brief = brief
        };

        evt.Brief.RequiredSkills.Should().HaveCount(3);
        evt.Brief.Title.Should().Be("Cloud Migration");
    }

    [Fact]
    public void ProjectAwardedEvent_SerializesCorrectly()
    {
        var evt = new ProjectAwardedEvent
        {
            EventId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            ProjectId = Guid.NewGuid(),
            VendorId = Guid.NewGuid(),
            ProposalId = Guid.NewGuid(),
            AwardedAmount = 50000.00m,
            Currency = "USD"
        };

        var json = JsonSerializer.Serialize(evt);
        var deserialized = JsonSerializer.Deserialize<ProjectAwardedEvent>(json);

        deserialized.Should().NotBeNull();
        deserialized!.AwardedAmount.Should().Be(50000.00m);
        deserialized.Currency.Should().Be("USD");
        deserialized.EventId.Should().Be(evt.EventId);
    }

    [Fact]
    public void MilestoneApprovedEvent_SerializesCorrectly()
    {
        var evt = new MilestoneApprovedEvent
        {
            EventId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            ProjectId = Guid.NewGuid(),
            MilestoneId = Guid.NewGuid(),
            VendorId = Guid.NewGuid(),
            PayoutAmount = 10000.00m,
            Currency = "EUR"
        };

        var json = JsonSerializer.Serialize(evt);
        var deserialized = JsonSerializer.Deserialize<MilestoneApprovedEvent>(json);

        deserialized.Should().NotBeNull();
        deserialized!.PayoutAmount.Should().Be(10000.00m);
    }

    [Fact]
    public void PaymentReceivedEvent_IncludesStripeReference()
    {
        var evt = new PaymentReceivedEvent
        {
            EventId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            InvoiceId = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            Amount = 25000m,
            Currency = "USD",
            StripePaymentIntentId = "pi_1234567890"
        };

        evt.StripePaymentIntentId.Should().StartWith("pi_");
    }

    [Fact]
    public void PayoutProcessedEvent_SerializesStatusAsString()
    {
        var evt = new PayoutProcessedEvent
        {
            EventId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            PayoutId = Guid.NewGuid(),
            VendorId = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            Amount = 15000m,
            Currency = "GBP",
            Status = PayoutStatus.Completed,
            WiseTransferId = "wise-tx-123"
        };

        var json = JsonSerializer.Serialize(evt);
        json.Should().Contain("\"Completed\"");
    }

    [Fact]
    public void ProjectStatusChangedEvent_SerializesEnumsAsStrings()
    {
        var evt = new ProjectStatusChangedEvent
        {
            EventId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            ProjectId = Guid.NewGuid(),
            OldStatus = ProjectStatus.Proposed,
            NewStatus = ProjectStatus.Awarded,
            ChangedBy = Guid.NewGuid()
        };

        var json = JsonSerializer.Serialize(evt);
        json.Should().Contain("\"Proposed\"");
        json.Should().Contain("\"Awarded\"");
    }

    [Fact]
    public void UserRegisteredEvent_SerializesRoleAsString()
    {
        var evt = new UserRegisteredEvent
        {
            EventId = Guid.NewGuid(),
            CorrelationId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UserId = Guid.NewGuid(),
            Email = "vendor@example.com",
            Role = UserRole.VendorContact
        };

        var json = JsonSerializer.Serialize(evt);
        json.Should().Contain("\"VendorContact\"");
    }
}
