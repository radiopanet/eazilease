using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaziLease.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverInclusionToLease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedDriverId",
                table: "VehicleLeases",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DriverFee",
                table: "VehicleLeases",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DriverId",
                table: "VehicleLeases",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncludeDriver",
                table: "VehicleLeases",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleLeases_DriverId",
                table: "VehicleLeases",
                column: "DriverId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLeases_Drivers_DriverId",
                table: "VehicleLeases",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLeases_Drivers_DriverId",
                table: "VehicleLeases");

            migrationBuilder.DropIndex(
                name: "IX_VehicleLeases_DriverId",
                table: "VehicleLeases");

            migrationBuilder.DropColumn(
                name: "AssignedDriverId",
                table: "VehicleLeases");

            migrationBuilder.DropColumn(
                name: "DriverFee",
                table: "VehicleLeases");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "VehicleLeases");

            migrationBuilder.DropColumn(
                name: "IncludeDriver",
                table: "VehicleLeases");
        }
    }
}
