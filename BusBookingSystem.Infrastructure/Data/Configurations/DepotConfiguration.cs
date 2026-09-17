/*
    This code defines the configuration for the Depot entity in the BusBookingSystem application using Entity Framework Core. 
    It specifies the primary key, required properties, maximum lengths, unique constraints, and default values for various fields. 
    Additionally, it configures the owned Address entity and sets up indexes for performance optimization.
*/

using BusBookingSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBookingSystem.Infrastructure.Data.Configurations
{
    public class DepotConfiguration : IEntityTypeConfiguration<Depot>
    {
        public void Configure(EntityTypeBuilder<Depot> depot)
        {
            //throw new NotImplementedException();

            depot.HasKey(d => d.Id);
            depot.Property(d => d.DepotCode).IsRequired().HasMaxLength(20);
            depot.Property(d => d.Name).IsRequired().HasMaxLength(100);
            depot.OwnsOne(d => d.Address, address =>
            {
                address.Property(a => a.Addressline1).HasMaxLength(200);
                address.Property(a => a.Addressline2).HasMaxLength(200);
                address.Property(a => a.City).IsRequired().HasMaxLength(100);
                address.Property(a => a.Region).IsRequired().HasMaxLength(100);
                address.Property(a => a.Country).IsRequired().HasMaxLength(100);
                address.Property(a => a.PostalCode).HasMaxLength(20);
            });
            depot.Property(d => d.PhoneNumber).HasMaxLength(20);
            depot.Property(d => d.Latitude).HasMaxLength(50);
            depot.Property(d => d.Longitude).HasMaxLength(50);
            depot.Property(d => d.IsActive).HasDefaultValue(true);
            depot.Property(d => d.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            depot.Property(d => d.UpdatedAt).IsRequired(false);

            // Indexes for performance optimization
            depot.HasIndex(d => d.DepotCode).IsUnique();

            //TODO: Consider composite index on Depot
        }
    }
}
