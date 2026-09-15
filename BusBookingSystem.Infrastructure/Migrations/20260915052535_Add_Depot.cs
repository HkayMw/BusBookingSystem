using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBookingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Depot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Depots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepotCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address_Addressline1 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address_Addressline2 = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address_Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address_Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address_PostalCode = table.Column<int>(type: "int", maxLength: 20, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Latitude = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Longitude = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Depots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Depots_DepotCode",
                table: "Depots",
                column: "DepotCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Depots");
        }
    }
}
