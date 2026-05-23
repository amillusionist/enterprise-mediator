using EnterpriseMediator.AiWorker.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnterpriseMediator.AiWorker.Infrastructure.Persistence;

/// <summary>
/// EF Core implementation of ISowRepository for managing SOW entities during processing.
/// </summary>
public class SowRepository : ISowRepository
{
    private readonly SowDbContext _context;
    private readonly ILogger<SowRepository> _logger;

    public SowRepository(SowDbContext context, ILogger<SowRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<SowEntity?> GetByIdAsync(Guid sowId, CancellationToken cancellationToken = default)
    {
        return await _context.SowDocuments.FindAsync(new object[] { sowId }, cancellationToken);
    }

    public async Task UpdateAsync(SowEntity entity, CancellationToken cancellationToken = default)
    {
        _context.SowDocuments.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("SOW entity {SowId} updated with status {Status}", entity.Id, entity.Status);
    }
}

/// <summary>
/// EF Core DbContext for the AI Worker's SOW processing data.
/// Connects to the same PostgreSQL instance as the Project Management Service.
/// </summary>
public class SowDbContext : DbContext
{
    public SowDbContext(DbContextOptions<SowDbContext> options) : base(options) { }

    public DbSet<SowEntity> SowDocuments => Set<SowEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SowEntity>(entity =>
        {
            entity.ToTable("sow_documents");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProjectId).HasColumnName("project_id");
            entity.Property(e => e.Status).HasColumnName("status").HasMaxLength(50);
            entity.Property(e => e.SanitizedContent).HasColumnName("sanitized_content");
            entity.Property(e => e.ExtractedDataJson).HasColumnName("extracted_data_json").HasColumnType("jsonb");
            entity.Property(e => e.VectorEmbeddings).HasColumnName("vector_embeddings");
            entity.Property(e => e.ErrorMessage).HasColumnName("error_message");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.ProcessedAt).HasColumnName("processed_at");
        });
    }
}
