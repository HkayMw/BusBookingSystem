/*
    This code defines a configuration class for the Bus entity in the BusBookingSystem application.
    It specifies the primary key, required properties, maximum lengths, and default values for various fields.
    Additionally, it configures the relationship with the Depot entity, enforces data integrity with check constraints, and creates indexes for performance optimization.
*/

using BusBookingSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBookingSystem.Infrastructure.Data.Configurations
{
    public class BusConfiguration : IEntityTypeConfiguration<Bus>
    {
        public void Configure(EntityTypeBuilder<Bus> bus)
        {
            //throw new NotImplementedException();
            bus.HasKey(b => b.Id);
            bus.Property(b => b.FleetNumber)
                .HasMaxLength(50)
                .IsRequired(false);
            bus.Property(b => b.Model)
                .IsRequired()
                .HasMaxLength(100);
            bus.Property(b => b.NumberOfSeats)
                .IsRequired();
            bus.Property(b => b.HasCargoSpace)
                .IsRequired();
            bus.Property(b => b.BusType)
                .IsRequired();
            bus.Property(b => b.BusStatus)
                .IsRequired();
            bus.Property(b => b.RegistrationNumber)
                .IsRequired()
                .HasMaxLength(20);
            bus.Property(b => b.ManufacturerName)
                .IsRequired()
                .HasMaxLength(100);
            bus.Property(b => b.RowVersion)
                .IsRowVersion();
            bus.Property(b => b.CreatedAt)
                .HasColumnType("datetimeoffset")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            bus.Property(b => b.UpdatedAt)
                .HasColumnType("datetimeoffset")
                .IsRequired(false);

            // Configure relationship with Depot entity
            bus.HasOne(b => b.HomeDepot)
                .WithMany()
                .HasForeignKey(b => b.HomeDepotId)
                .OnDelete(DeleteBehavior.Restrict);

            // enforce data integrity with constraints
            bus.ToTable(b => b.HasCheckConstraint("CK_Bus_NumberOfSeats_NonNegative", "[NumberOfSeats] > 0"));
            bus.ToTable(b => b.HasCheckConstraint("CK_Bus_ManufactureYear_Reasonable", "[ManufactureYear] >= 1900 AND [ManufactureYear] <= YEAR(GETUTCDATE()) + 1"));
            bus.ToTable(b => b.HasCheckConstraint("CK_Bus_CargoCapacity_NonNegative", "[CargoCapacity] >= 0"));
            // Indexes for performance optimization
            bus.HasIndex(b => b.RegistrationNumber).IsUnique();
            bus.HasIndex(b => b.FleetNumber);
            bus.HasIndex(b => b.HomeDepotId);


        }
    }
}
