using EnterpriseMediator.Financial.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseMediator.Financial.Infrastructure.Persistence.Configurations;

public class PayoutConfiguration : IEntityTypeConfiguration<Payout>
{
    public void Configure(EntityTypeBuilder<Payout> builder)
    {
        builder.ToTable("Payouts");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(p => p.VendorId).IsRequired();
        builder.Property(p => p.ProjectId).IsRequired();

        builder.Property(p => p.Status)
            .HasConversion(
                v => v.ToString(),
                v => (PayoutStatus)Enum.Parse(typeof(PayoutStatus), v))
            .HasMaxLength(50)
            .IsRequired();

        builder.ComplexProperty(p => p.Amount, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();
            money.ComplexProperty(m => m.Currency, currency =>
            {
                currency.Property(c => c.Code)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsFixedLength()
                    .IsRequired();
            });
        });

        builder.Property(p => p.WiseTransferId)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.Property(p => p.ApproverId)
            .IsRequired(false);

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.ProcessedAt).IsRequired(false);

        builder.Property(p => p.FailureReason)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(p => p.RowVersion)
            .IsRowVersion();

        builder.HasIndex(p => p.ProjectId)
            .HasDatabaseName("IX_Payouts_ProjectId");

        builder.HasIndex(p => p.VendorId)
            .HasDatabaseName("IX_Payouts_VendorId");

        builder.HasIndex(p => p.Status)
            .HasDatabaseName("IX_Payouts_Status");

        builder.HasMany(p => p.Transactions)
            .WithOne()
            .HasForeignKey("PayoutId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(p => p.DomainEvents);
    }
}
