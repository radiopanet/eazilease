using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaziLease.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleLeaseColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "VehicleLeases");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "VehicleLeases",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "VehicleLeases");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "VehicleLeases",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
