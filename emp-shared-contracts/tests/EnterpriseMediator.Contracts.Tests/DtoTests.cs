using System.Text.Json;
using EnterpriseMediator.Contracts.Common;
using EnterpriseMediator.Contracts.DTOs.Financials;
using EnterpriseMediator.Contracts.DTOs.Projects;
using EnterpriseMediator.Contracts.DTOs.Users;
using EnterpriseMediator.Contracts.DTOs.Vendors;
using EnterpriseMediator.Contracts.Enums;

namespace EnterpriseMediator.Contracts.Tests;

public class DtoTests
{
    [Fact]
    public void ProjectDto_SerializesStatusAsString()
    {
        var dto = new ProjectDto
        {
            Id = Guid.NewGuid(),
            Name = "Test Project",
            ClientId = Guid.NewGuid(),
            Status = ProjectStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var json = JsonSerializer.Serialize(dto);
        json.Should().Contain("\"Active\"");

        var deserialized = JsonSerializer.Deserialize<ProjectDto>(json);
        deserialized!.Status.Should().Be(ProjectStatus.Active);
    }

    [Fact]
    public void PagedResultDto_CalculatesPageMetadata()
    {
        var result = new PagedResultDto<string>
        {
            Items = new[] { "a", "b", "c" },
            TotalCount = 25,
            Page = 2,
            PageSize = 10
        };

        result.TotalPages.Should().Be(3);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void PagedResultDto_FirstPage_HasNoPreviousPage()
    {
        var result = new PagedResultDto<int>
        {
            Items = new[] { 1 },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };

        result.TotalPages.Should().Be(1);
        result.HasNextPage.Should().BeFalse();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void FinancialSummaryDto_RoundTripsViaJson()
    {
        var dto = new FinancialSummaryDto
        {
            ProjectId = Guid.NewGuid(),
            TotalBudget = 100000m,
            TotalInvoiced = 50000m,
            TotalPaid = 25000m,
            PendingPayouts = 10000m,
            Currency = "USD",
            HasOverdueInvoices = true
        };

        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<FinancialSummaryDto>(json);

        deserialized.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public void InvoiceDto_SerializesStatusAsString()
    {
        var dto = new InvoiceDto
        {
            Id = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            Amount = 5000m,
            Currency = "USD",
            Status = InvoiceStatus.Overdue,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var json = JsonSerializer.Serialize(dto);
        json.Should().Contain("\"Overdue\"");
    }

    [Fact]
    public void StandardizedErrorDto_SupportsValidationDetails()
    {
        var error = new StandardizedErrorDto
        {
            TraceId = "abc-123",
            Code = "VALIDATION_ERROR",
            Message = "One or more validation errors occurred.",
            Details = new Dictionary<string, string[]>
            {
                ["Name"] = new[] { "Name is required.", "Name cannot exceed 100 characters." },
                ["ClientId"] = new[] { "Client ID is required." }
            }
        };

        var json = JsonSerializer.Serialize(error);
        var deserialized = JsonSerializer.Deserialize<StandardizedErrorDto>(json);

        deserialized!.Details.Should().HaveCount(2);
        deserialized.Details!["Name"].Should().HaveCount(2);
    }

    [Fact]
    public void VendorMatchResultDto_SerializesCorrectly()
    {
        var dto = new VendorMatchResultDto
        {
            VendorId = Guid.NewGuid(),
            CompanyName = "Acme Corp",
            SimilarityScore = 0.92,
            MatchedSkills = new[] { "C#", ".NET", "Azure" }
        };

        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<VendorMatchResultDto>(json);

        deserialized!.SimilarityScore.Should().BeApproximately(0.92, 0.001);
        deserialized.MatchedSkills.Should().HaveCount(3);
    }

    [Fact]
    public void UserInvitationDto_SerializesRoleAsString()
    {
        var dto = new UserInvitationDto
        {
            Email = "test@example.com",
            Role = UserRole.ClientContact,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(7),
            IsValid = true
        };

        var json = JsonSerializer.Serialize(dto);
        json.Should().Contain("\"ClientContact\"");
    }

    [Fact]
    public void MilestoneDto_RoundTripsViaJson()
    {
        var dto = new MilestoneDto
        {
            Id = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            Title = "Phase 1 Delivery",
            Amount = 25000m,
            Currency = "USD",
            Status = MilestoneStatus.Pending,
            DueDate = DateTimeOffset.UtcNow.AddMonths(1)
        };

        var json = JsonSerializer.Serialize(dto);
        var deserialized = JsonSerializer.Deserialize<MilestoneDto>(json);

        deserialized.Should().BeEquivalentTo(dto);
    }
}
