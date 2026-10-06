using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameBookingSeatsCheckConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Booking_NumberOfSeats_greaterThanZero",
                table: "Bookings");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Booking_NumberOfSeats_NonNegative",
                table: "Bookings",
                sql: "[NumberOfSeats] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Booking_NumberOfSeats_NonNegative",
                table: "Bookings");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Booking_NumberOfSeats_greaterThanZero",
                table: "Bookings",
                sql: "[NumberOfSeats] >= 0");
        }
    }
}
