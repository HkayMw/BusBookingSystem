/*
    This code defines the configuration for the Trip entity in the BusBookingSystem application using Entity Framework Core.
    It specifies the primary key, required properties, precision for decimal fields, default values, and concurrency control using a row version.
    Additionally, it sets up foreign key relationships with the Bus and Route entities, enforces check constraints for data integrity, and creates indexes for performance optimization.
*/

using BusBookingSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBookingSystem.Infrastructure.Data.Configurations
{
    public class TripConfiguration : IEntityTypeConfiguration<Trip>
    {
        public void Configure(EntityTypeBuilder<Trip> trip)
        {
            trip.HasKey(t => t.Id);

            trip.Property(t => t.SeatFare)
                .HasPrecision(18, 2)
                .IsRequired();

            trip.Property(t => t.CargoFare)
                .HasPrecision(18, 2);

            trip.Property(t => t.SeatsBooked)
                .HasDefaultValue(0)
                .IsRequired();

            trip.Property(t => t.RowVersion)
                .IsRowVersion();

            trip.Property(t => t.DepartureTime)
                .HasColumnType("datetimeoffset")
                .IsRequired();

            trip.Property(t => t.ArrivalTime)
                .HasColumnType("datetimeoffset");

            trip.Property(t => t.CreatedAt)
                .HasColumnType("datetimeoffset")
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();

            trip.Property(t => t.UpdatedAt)
                .HasColumnType("datetimeoffset")
                .IsRequired(false);

            trip.HasOne(t => t.Bus)
                .WithMany()
                .HasForeignKey(t => t.BusId)
                .OnDelete(DeleteBehavior.Restrict);

            trip.HasOne(t => t.Route)
                .WithMany()
                .HasForeignKey(t => t.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enforce data integrity with check constraints
            trip.ToTable(t => t.HasCheckConstraint("CK_Trip_DepartureTime_ArrivalTime", "[DepartureTime] < [ArrivalTime]"));
            trip.ToTable(t => t.HasCheckConstraint("CK_Trip_SeatsBooked_NonNegative", "[SeatsBooked] >= 0"));
            trip.ToTable(t => t.HasCheckConstraint("CK_Trip_SeatFare_NonNegative", "[SeatFare] >= 0"));
            trip.ToTable(t => t.HasCheckConstraint("CK_Trip_CargoFare_NonNegative", "[CargoFare] >= 0"));
            trip.ToTable(t => t.HasCheckConstraint("CK_Trip_CargoCapacityBooked_NonNegative", "[CargoCapacityBooked] >= 0"));

            // Indexes for performance optimization
            trip.HasIndex(t => t.DepartureTime);
            trip.HasIndex(t => t.BusId);
            trip.HasIndex(t => t.RouteId);

            // TODO: Consider composite indexes for frequently queried combinations, e.g., DepartureTime + RouteId
        }
    }
}