using BusBookingSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBookingSystem.Infrastructure.Data.Configurations
{
    public class DepotConfiguration : IEntityTypeConfiguration<Depot>
    {
        public void Configure(EntityTypeBuilder<Depot> entity)
        {
            //throw new NotImplementedException();

            entity.HasKey(d => d.Id);
            entity.Property(d => d.DepotCode).IsRequired().HasMaxLength(20);
            entity.HasIndex(d=> d.DepotCode).IsUnique();
            entity.Property(d => d.Name).IsRequired().HasMaxLength(100);
            entity.OwnsOne(d => d.Address, address =>
            {
                address.Property(a => a.Addressline1).IsRequired().HasMaxLength(200);
                address.Property(a => a.Addressline2).HasMaxLength(200);
                address.Property(a => a.City).IsRequired().HasMaxLength(100);
                address.Property(a => a.Region).IsRequired().HasMaxLength(100);
                address.Property(a => a.Country).IsRequired().HasMaxLength(100);
                address.Property(a => a.PostalCode).IsRequired().HasMaxLength(20);
            });
            entity.Property(d => d.PhoneNumber).HasMaxLength(20);
            entity.Property(d => d.Latitude).HasMaxLength(50);
            entity.Property(d => d.Longitude).HasMaxLength(50);
            entity.Property(d => d.IsActive).HasDefaultValue(true);
            entity.Property(d => d.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            //entity.Property(d => d.UpdatedAt).IsRequired(false);
        }
    }
}
