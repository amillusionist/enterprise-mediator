using System.ComponentModel.DataAnnotations;

namespace EnterpriseMediator.Contracts.DTOs.Projects;

/// <summary>
/// Payload for initiating a project. Used by the Project API.
/// </summary>
public record CreateProjectRequest
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; init; }

    [Required]
    public required Guid ClientId { get; init; }

    [MaxLength(1000)]
    public string? Description { get; init; }

    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
}
