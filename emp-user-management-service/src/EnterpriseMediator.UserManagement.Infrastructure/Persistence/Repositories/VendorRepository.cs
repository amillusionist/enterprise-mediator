using EnterpriseMediator.UserManagement.Domain.Aggregates.Vendor;
using EnterpriseMediator.UserManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.UserManagement.Infrastructure.Persistence.Repositories;

public class VendorRepository : IVendorRepository
{
    private readonly UserDbContext _dbContext;
    private readonly ILogger<VendorRepository> _logger;

    public VendorRepository(UserDbContext dbContext, ILogger<VendorRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Vendor?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.Vendors
            .Include(v => v.Skills)
            .FirstOrDefaultAsync(v => v.Id == id, ct);
    }

    public async Task<Vendor?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalizedEmail = email.ToLowerInvariant().Trim();
        return await _dbContext.Vendors
            .Include(v => v.Skills)
            .FirstOrDefaultAsync(v => v.PrimaryContactEmail == normalizedEmail, ct);
    }

    public async Task AddAsync(Vendor vendor, CancellationToken ct = default)
    {
        await _dbContext.Vendors.AddAsync(vendor, ct);
        _logger.LogDebug("Vendor {VendorId} added to context", vendor.Id);
    }

    public void Update(Vendor vendor)
    {
        _dbContext.Vendors.Update(vendor);
        _logger.LogDebug("Vendor {VendorId} marked for update", vendor.Id);
    }

    public async Task<IEnumerable<Vendor>> ListByStatusAsync(string status, int page, int pageSize, CancellationToken ct = default)
    {
        return await _dbContext.Vendors
            .Include(v => v.Skills)
            .Where(v => v.VettingStatus == status)
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Vendor>> GetBySkillAsync(string skillTag, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(skillTag))
            return Enumerable.Empty<Vendor>();

        var normalizedTag = skillTag.Trim();
        return await _dbContext.Vendors
            .Include(v => v.Skills)
            .Where(v => v.Skills.Any(s => EF.Functions.ILike(s.Name, normalizedTag)))
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}
