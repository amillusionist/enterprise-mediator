using EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;
using EnterpriseMediator.ProjectManagement.Domain.Enums;
using EnterpriseMediator.ProjectManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.ProjectManagement.Infrastructure.Persistence.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ProjectDbContext _context;
    private readonly ILogger<ProjectRepository> _logger;

    public ProjectRepository(ProjectDbContext context, ILogger<ProjectRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IUnitOfWork UnitOfWork => _context;

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Projects.FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Project?> GetByIdWithProposalsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Projects.Include(p => p.Proposals).FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Project?> GetByIdWithMilestonesAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Projects.Include(p => p.Milestones).FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<Project?> GetByIdFullAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Projects
            .Include(p => p.Proposals).Include(p => p.Milestones)
            .Include(p => p.SowDocument).Include(p => p.PayoutRules)
            .AsSplitQuery().FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<IReadOnlyList<Project>> GetByClientIdAsync(Guid clientId, CancellationToken ct = default)
    {
        return await _context.Projects.Where(p => p.ClientId == clientId).OrderByDescending(p => p.CreatedAt).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Project>> GetByStatusAsync(ProjectStatus status, CancellationToken ct = default)
    {
        return await _context.Projects.Where(p => p.Status == status).OrderByDescending(p => p.CreatedAt).ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Project> Projects, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? status,
        Guid? clientId,
        CancellationToken ct = default)
    {
        var query = _context.Projects
            .Include(p => p.Proposals)
            .Include(p => p.Milestones)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<ProjectStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            query = query.Where(p => p.Status == parsedStatus);
        }

        if (clientId.HasValue)
        {
            query = query.Where(p => p.ClientId == clientId.Value);
        }

        var totalCount = await query.CountAsync(ct);

        var projects = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (projects, totalCount);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Projects.AnyAsync(p => p.Id == id, ct);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeProjectId = null, CancellationToken ct = default)
    {
        var query = _context.Projects.Where(p => p.Name == name);
        if (excludeProjectId.HasValue)
            query = query.Where(p => p.Id != excludeProjectId.Value);
        return await query.AnyAsync(ct);
    }

    public async Task AddAsync(Project project, CancellationToken ct = default)
    {
        _logger.LogDebug("Adding project {ProjectId}", project.Id);
        await _context.Projects.AddAsync(project, ct);
    }

    public void Update(Project project)
    {
        _logger.LogDebug("Updating project {ProjectId}", project.Id);
        _context.Projects.Update(project);
    }
}
