using EnterpriseMediator.ProjectManagement.Domain.Aggregates.ProjectAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseMediator.ProjectManagement.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.ClientId).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(2000);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.FixedMargin).HasColumnType("decimal(18,2)");
        builder.Property(p => p.PercentageMargin).HasColumnType("decimal(5,2)");

        builder.OwnsOne(p => p.SowDetails, sowBuilder => { sowBuilder.ToJson(); });

        builder.HasOne(p => p.SowDocument).WithOne().HasForeignKey<SowDocument>(s => s.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Proposals).WithOne().HasForeignKey(pr => pr.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Milestones).WithOne().HasForeignKey(m => m.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.PayoutRules).WithOne().HasForeignKey(r => r.ProjectId).OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(p => p.DomainEvents);
        builder.Property<byte[]>("RowVersion").IsRowVersion();
        builder.HasIndex(p => p.ClientId);
        builder.HasIndex(p => p.Status);
    }
}

public class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
    {
        builder.ToTable("Proposals");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();
        builder.Property(p => p.VendorId).IsRequired();
        builder.Property(p => p.ProposedCost).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(p => p.Currency).HasMaxLength(3).IsRequired();
        builder.Property(p => p.Timeline).HasMaxLength(500).IsRequired();
        builder.Property(p => p.KeyPersonnel).HasMaxLength(2000).IsRequired();
        builder.Property(p => p.CoverLetter).HasMaxLength(5000);
        builder.Property(p => p.ProposalDocumentUrl).HasMaxLength(2000);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(p => p.InternalFlag).HasMaxLength(100);
        builder.HasIndex(p => p.VendorId);
        builder.HasIndex(p => new { p.ProjectId, p.VendorId }).IsUnique();
    }
}

public class SowDocumentConfiguration : IEntityTypeConfiguration<SowDocument>
{
    public void Configure(EntityTypeBuilder<SowDocument> builder)
    {
        builder.ToTable("SowDocuments");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.FileName).HasMaxLength(500).IsRequired();
        builder.Property(s => s.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(s => s.StorageKey).HasMaxLength(1000).IsRequired();
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(s => s.FailureReason).HasMaxLength(2000);
    }
}

public class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
{
    public void Configure(EntityTypeBuilder<Milestone> builder)
    {
        builder.ToTable("Milestones");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.Property(m => m.Title).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(2000);
        builder.Property(m => m.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(m => m.Currency).HasMaxLength(3).IsRequired();
        builder.Property(m => m.Status).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.HasIndex(m => m.ProjectId);
    }
}

public class ProjectPayoutRuleConfiguration : IEntityTypeConfiguration<ProjectPayoutRule>
{
    public void Configure(EntityTypeBuilder<ProjectPayoutRule> builder)
    {
        builder.ToTable("ProjectPayoutRules");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.MilestoneName).HasMaxLength(200).IsRequired();
        builder.Property(r => r.Percentage).HasColumnType("decimal(5,2)").IsRequired();
    }
}
