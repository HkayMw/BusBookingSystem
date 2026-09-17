/*
    This code defines the configuration for the Route entity in the BusBookingSystem application.
    It uses Entity Framework Core to configure the properties, relationships, and constraints of the Route entity.
    The configuration ensures data integrity, enforces required fields, and optimizes performance with indexes.
*/

using BusBookingSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBookingSystem.Infrastructure.Data.Configurations
{
    public class RouteConfiguration : IEntityTypeConfiguration<Route>
    {
        public void Configure(EntityTypeBuilder<Route> route)
        {
            route.HasKey(r => r.Id);
            route.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);
            route.Property(r => r.CreatedAt)
                .HasColumnType("datetimeoffset")
                .HasDefaultValueSql("SYSUTCDATETIME()")
                .IsRequired();
            route.Property(r => r.UpdatedAt)
                .HasColumnType("datetimeoffset")
                .IsRequired(false);
            route.HasOne(r => r.Origin)
                .WithMany()
                .HasForeignKey(r => r.OriginId)
                .OnDelete(DeleteBehavior.Restrict);
            route.HasOne(r => r.Destination)
                .WithMany()
                .HasForeignKey(r => r.DestinationId)
                .OnDelete(DeleteBehavior.Restrict);
            // Enforce data integrity with check constraints
            route.ToTable(t => t.HasCheckConstraint("CK_Route_Origin_Destination", "[OriginId] <> [DestinationId]"));
            // Indexes for performance optimization
            route.HasIndex(r => r.OriginId);
            route.HasIndex(r => r.DestinationId);
        }
    }
}
