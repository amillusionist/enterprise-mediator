using EnterpriseMediator.UserManagement.Domain.Aggregates.Vendor;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseMediator.UserManagement.Infrastructure.Persistence.Configurations;

public class VendorConfiguration : IEntityTypeConfiguration<Vendor>
{
    public void Configure(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(v => v.CompanyName)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(v => v.CompanyName);

        builder.Property(v => v.PrimaryContactEmail)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(v => v.PrimaryContactEmail);

        builder.Property(v => v.VettingStatus)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(v => v.VettingStatus);

        builder.Property(v => v.CreatedAt)
            .IsRequired();

        builder.Property(v => v.UpdatedAt)
            .IsRequired(false);

        // Address Value Object (Owned Type)
        builder.OwnsOne(v => v.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street).HasColumnName("Address_Street").HasMaxLength(200).IsRequired();
            addressBuilder.Property(a => a.City).HasColumnName("Address_City").HasMaxLength(100).IsRequired();
            addressBuilder.Property(a => a.State).HasColumnName("Address_State").HasMaxLength(100);
            addressBuilder.Property(a => a.PostalCode).HasColumnName("Address_PostalCode").HasMaxLength(20);
            addressBuilder.Property(a => a.Country).HasColumnName("Address_Country").HasMaxLength(100).IsRequired();
        });

        // PaymentInfo Value Object (Owned Type) - encrypted at rest via ValueConverter in production
        builder.OwnsOne(v => v.PaymentDetails, paymentBuilder =>
        {
            paymentBuilder.Property(p => p.ProviderName)
                .HasColumnName("Payment_ProviderName")
                .HasMaxLength(150)
                .IsRequired(false);

            paymentBuilder.Property(p => p.AccountIdentifier)
                .HasColumnName("Payment_AccountIdentifier")
                .HasMaxLength(500)
                .IsRequired(false);

            paymentBuilder.Property(p => p.RoutingIdentifier)
                .HasColumnName("Payment_RoutingIdentifier")
                .HasMaxLength(200)
                .IsRequired(false);

            paymentBuilder.Property(p => p.Currency)
                .HasColumnName("Payment_Currency")
                .HasMaxLength(3)
                .IsRequired(false);
        });

        // VendorSkill Collection (Owned Many)
        builder.OwnsMany(v => v.Skills, skillBuilder =>
        {
            skillBuilder.ToTable("VendorSkills");

            skillBuilder.WithOwner().HasForeignKey(s => s.VendorId);

            skillBuilder.HasKey(s => s.Id);

            skillBuilder.Property(s => s.Id)
                .ValueGeneratedNever();

            skillBuilder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);

            skillBuilder.HasIndex(s => s.Name);
        });

        builder.Ignore(v => v.DomainEvents);
    }
}
