using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaziLease.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaseExtensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VehicleLeases_VehicleId",
                table: "VehicleLeases");

            migrationBuilder.AddColumn<int>(
                name: "ExtensionCount",
                table: "VehicleLeases",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsExtended",
                table: "VehicleLeases",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLeases_VehicleId",
                table: "VehicleLeases",
                column: "VehicleId",
                unique: true,
                filter: "\"ReturnDate\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VehicleLeases_VehicleId",
                table: "VehicleLeases");

            migrationBuilder.DropColumn(
                name: "ExtensionCount",
                table: "VehicleLeases");

            migrationBuilder.DropColumn(
                name: "IsExtended",
                table: "VehicleLeases");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLeases_VehicleId",
                table: "VehicleLeases",
                column: "VehicleId");
        }
    }
}
