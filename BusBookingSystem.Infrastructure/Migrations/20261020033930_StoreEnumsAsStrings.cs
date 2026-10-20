using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StoreEnumsAsStrings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "BusType",
                table: "Buses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "BusStatus",
                table: "Buses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "BookingStatus",
                table: "Bookings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql("""
                UPDATE Bookings SET BookingStatus = CASE BookingStatus
                    WHEN '0' THEN 'Pending' WHEN '1' THEN 'Confirmed' WHEN '2' THEN 'Cancelled' END;

                UPDATE Buses SET BusType = CASE BusType
                    WHEN '0' THEN 'Coaster' WHEN '1' THEN 'Coach' WHEN '2' THEN 'Minibus' WHEN '3' THEN 'DoubleDecker' END;

                UPDATE Buses SET BusStatus = CASE BusStatus
                    WHEN '0' THEN 'Available' WHEN '1' THEN 'InUse' WHEN '2' THEN 'UnderMaintenance'
                    WHEN '3' THEN 'OutOfService' WHEN '4' THEN 'Retired' END;
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE Bookings SET BookingStatus = CASE BookingStatus
                    WHEN 'Pending' THEN '0' WHEN 'Confirmed' THEN '1' WHEN 'Cancelled' THEN '2' END;

                UPDATE Buses SET BusType = CASE BusType
                    WHEN 'Coaster' THEN '0' WHEN 'Coach' THEN '1' WHEN 'Minibus' THEN '2' WHEN 'DoubleDecker' THEN '3' END;

                UPDATE Buses SET BusStatus = CASE BusStatus
                    WHEN 'Available' THEN '0' WHEN 'InUse' THEN '1' WHEN 'UnderMaintenance' THEN '2'
                    WHEN 'OutOfService' THEN '3' WHEN 'Retired' THEN '4' END;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "BusType",
                table: "Buses",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "BusStatus",
                table: "Buses",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "BookingStatus",
                table: "Bookings",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
