namespace EnterpriseMediator.Contracts.DTOs.Vendors;

/// <summary>
/// Individual vendor skill with optional embedding vector reference.
/// </summary>
public record VendorSkillDto
{
    public required string Name { get; init; }
    public string? Category { get; init; }
    public int? YearsOfExperience { get; init; }
    public string? ProficiencyLevel { get; init; }
}
