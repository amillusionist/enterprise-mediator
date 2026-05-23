using EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;
using EnterpriseMediator.ProjectManagement.Domain.Enums;

namespace EnterpriseMediator.ProjectManagement.Domain.Interfaces;

public interface IProjectRepository
{
    IUnitOfWork UnitOfWork { get; }
    Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Project?> GetByIdWithProposalsAsync(Guid id, CancellationToken ct = default);
    Task<Project?> GetByIdWithMilestonesAsync(Guid id, CancellationToken ct = default);
    Task<Project?> GetByIdFullAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Project>> GetByClientIdAsync(Guid clientId, CancellationToken ct = default);
    Task<IReadOnlyList<Project>> GetByStatusAsync(ProjectStatus status, CancellationToken ct = default);
    Task<(IReadOnlyList<Project> Projects, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        Guid? clientId,
        CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeProjectId = null, CancellationToken ct = default);
    Task AddAsync(Project project, CancellationToken ct = default);
    void Update(Project project);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
