using EnterpriseMediator.UserManagement.Domain.Aggregates.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseMediator.UserManagement.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(c => c.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(c => c.CompanyName);

        builder.Property(c => c.PrimaryContactEmail)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(c => c.PrimaryContactEmail);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired(false);

        // Company Address (Owned Type)
        builder.OwnsOne(c => c.CompanyAddress, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street).HasColumnName("Company_Street").HasMaxLength(200).IsRequired();
            addressBuilder.Property(a => a.City).HasColumnName("Company_City").HasMaxLength(100).IsRequired();
            addressBuilder.Property(a => a.State).HasColumnName("Company_State").HasMaxLength(100);
            addressBuilder.Property(a => a.PostalCode).HasColumnName("Company_PostalCode").HasMaxLength(20);
            addressBuilder.Property(a => a.Country).HasColumnName("Company_Country").HasMaxLength(100).IsRequired();
        });

        // Billing Address (Owned Type)
        builder.OwnsOne(c => c.BillingAddress, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street).HasColumnName("Billing_Street").HasMaxLength(200).IsRequired();
            addressBuilder.Property(a => a.City).HasColumnName("Billing_City").HasMaxLength(100).IsRequired();
            addressBuilder.Property(a => a.State).HasColumnName("Billing_State").HasMaxLength(100);
            addressBuilder.Property(a => a.PostalCode).HasColumnName("Billing_PostalCode").HasMaxLength(20);
            addressBuilder.Property(a => a.Country).HasColumnName("Billing_Country").HasMaxLength(100).IsRequired();
        });

        builder.Ignore(c => c.DomainEvents);
    }
}
