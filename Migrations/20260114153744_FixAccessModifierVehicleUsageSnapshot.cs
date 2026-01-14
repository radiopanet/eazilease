using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaziLease.Migrations
{
    /// <inheritdoc />
    public partial class FixAccessModifierVehicleUsageSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RepairRecordsCount",
                table: "vehicleUsageSnapshots",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalMaintenanceRecords",
                table: "vehicleUsageSnapshots",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepairRecordsCount",
                table: "vehicleUsageSnapshots");

            migrationBuilder.DropColumn(
                name: "TotalMaintenanceRecords",
                table: "vehicleUsageSnapshots");
        }
    }
}
