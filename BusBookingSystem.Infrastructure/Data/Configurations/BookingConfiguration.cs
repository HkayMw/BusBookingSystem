

using BusBookingSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusBookingSystem.Infrastructure.Data.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> booking)
        {
            //throw new NotImplementedException();
            booking.HasKey(b => b.Id);
            booking.Property(b => b.NumberOfSeats)
                .IsRequired();
            booking.Property(b => b.BookingReference)
                .IsRequired()
                .HasMaxLength(10);
            booking.Property(b => b.BookingStatus)
                .IsRequired();
            booking.Property(b => b.RowVersion)
                .IsRowVersion();
            booking.Property(b => b.CreatedAt)
                .HasColumnType("datetimeoffset")
                .HasDefaultValueSql("SYSUTCDATETIME()");
            booking.Property(b => b.UpdatedAt)
                .HasColumnType("datetimeoffset")
                .IsRequired(false);


            // Relationships
            booking.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            booking.HasOne(b => b.Trip)
                .WithMany()
                .HasForeignKey(b => b.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enforce data integrity with check constraints
            booking.ToTable(b => b.HasCheckConstraint("CK_Booking_NumberOfSeats_greaterThanZero", "[NumberOfSeats] > 0"));

            // Indexes for performance optimization
            booking.HasIndex(b => b.BookingReference).IsUnique();



        }
    }
}
