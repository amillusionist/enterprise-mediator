using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseMediator.Financial.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(t => t.Type)
            .HasConversion(
                v => v.ToString(),
                v => (TransactionType)Enum.Parse(typeof(TransactionType), v))
            .HasMaxLength(50)
            .IsRequired();

        builder.ComplexProperty(t => t.Amount, money =>
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

        builder.Property(t => t.Timestamp).IsRequired();
        builder.Property(t => t.ProjectId).IsRequired();

        builder.Property(t => t.InvoiceId).IsRequired(false);
        builder.Property(t => t.PayoutId).IsRequired(false);

        builder.Property(t => t.ExternalReferenceId)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.HasIndex(t => t.ProjectId)
            .HasDatabaseName("IX_Transactions_ProjectId");

        builder.HasIndex(t => t.ExternalReferenceId)
            .IsUnique()
            .HasDatabaseName("IX_Transactions_ExternalReferenceId");

        builder.HasIndex(t => new { t.Type, t.Timestamp })
            .HasDatabaseName("IX_Transactions_Type_Timestamp");
    }
}
