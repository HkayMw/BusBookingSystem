using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class modified_for_Trip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Routes");

            migrationBuilder.AddColumn<int>(
                name: "CargoCapacityBooked",
                table: "Trips",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CargoCapacity",
                table: "Buses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Trip_CargoCapacityBooked_NonNegative",
                table: "Trips",
                sql: "[CargoCapacityBooked] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Bus_CargoCapacity_NonNegative",
                table: "Buses",
                sql: "[CargoCapacity] >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Trip_CargoCapacityBooked_NonNegative",
                table: "Trips");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Bus_CargoCapacity_NonNegative",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "CargoCapacityBooked",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "CargoCapacity",
                table: "Buses");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Routes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
