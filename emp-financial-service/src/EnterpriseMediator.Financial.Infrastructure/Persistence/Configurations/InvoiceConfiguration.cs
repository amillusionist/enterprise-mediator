using EnterpriseMediator.Financial.Domain.Entities;
using EnterpriseMediator.Financial.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseMediator.Financial.Infrastructure.Persistence.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoices");

            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id)
                .ValueGeneratedNever()
                .IsRequired();

            builder.Property(i => i.ProjectId).IsRequired();
            builder.Property(i => i.ClientId).IsRequired();

            builder.Property(i => i.Status)
                .HasConversion(
                    v => v.ToString(),
                    v => (InvoiceStatus)Enum.Parse(typeof(InvoiceStatus), v))
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(i => i.StripePaymentIntentId)
                .HasMaxLength(255)
                .IsRequired(false);

            builder.ComplexProperty(i => i.TotalAmount, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("TotalAmount")
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

            builder.Property(i => i.CreatedAt).IsRequired();
            builder.Property(i => i.PaidAt).IsRequired(false);

            builder.Property(i => i.RowVersion)
                .IsRowVersion();

            builder.HasIndex(i => i.ProjectId)
                .HasDatabaseName("IX_Invoices_ProjectId");

            builder.HasIndex(i => i.StripePaymentIntentId)
                .IsUnique()
                .HasFilter("\"StripePaymentIntentId\" IS NOT NULL")
                .HasDatabaseName("IX_Invoices_StripePaymentIntentId");

            builder.HasMany(i => i.Transactions)
                .WithOne()
                .HasForeignKey("InvoiceId")
                .OnDelete(DeleteBehavior.Restrict);

            builder.Ignore(i => i.DomainEvents);
        }
    }
}
