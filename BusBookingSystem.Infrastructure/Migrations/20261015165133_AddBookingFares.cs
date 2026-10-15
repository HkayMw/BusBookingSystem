using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingFares : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CargoFare",
                table: "Bookings",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SeatsFare",
                table: "Bookings",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalFare",
                table: "Bookings",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Booking_CargoFare_NonNegative",
                table: "Bookings",
                sql: "[CargoFare] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Booking_SeatsFare_NonNegative",
                table: "Bookings",
                sql: "[SeatsFare] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Booking_SeatsOrCargo_Valid",
                table: "Bookings",
                sql: "[NumberOfSeats] > 0 OR [CargoWeight] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Booking_TotalFare_NonNegative",
                table: "Bookings",
                sql: "[TotalFare] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Booking_TotalFare_Valid",
                table: "Bookings",
                sql: "[TotalFare] = [SeatsFare] + [CargoFare]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Booking_CargoFare_NonNegative",
                table: "Bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Booking_SeatsFare_NonNegative",
                table: "Bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Booking_SeatsOrCargo_Valid",
                table: "Bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Booking_TotalFare_NonNegative",
                table: "Bookings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Booking_TotalFare_Valid",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CargoFare",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "SeatsFare",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TotalFare",
                table: "Bookings");
        }
    }
}
