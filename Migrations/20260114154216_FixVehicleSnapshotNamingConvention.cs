using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaziLease.Migrations
{
    /// <inheritdoc />
    public partial class FixVehicleSnapshotNamingConvention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_vehicleUsageSnapshots_Vehicles_VehicleId",
                table: "vehicleUsageSnapshots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vehicleUsageSnapshots",
                table: "vehicleUsageSnapshots");

            migrationBuilder.RenameTable(
                name: "vehicleUsageSnapshots",
                newName: "VehicleUsageSnapshots");

            migrationBuilder.RenameIndex(
                name: "IX_vehicleUsageSnapshots_VehicleId",
                table: "VehicleUsageSnapshots",
                newName: "IX_VehicleUsageSnapshots_VehicleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleUsageSnapshots",
                table: "VehicleUsageSnapshots",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleUsageSnapshots_Vehicles_VehicleId",
                table: "VehicleUsageSnapshots",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleUsageSnapshots_Vehicles_VehicleId",
                table: "VehicleUsageSnapshots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleUsageSnapshots",
                table: "VehicleUsageSnapshots");

            migrationBuilder.RenameTable(
                name: "VehicleUsageSnapshots",
                newName: "vehicleUsageSnapshots");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleUsageSnapshots_VehicleId",
                table: "vehicleUsageSnapshots",
                newName: "IX_vehicleUsageSnapshots_VehicleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_vehicleUsageSnapshots",
                table: "vehicleUsageSnapshots",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_vehicleUsageSnapshots_Vehicles_VehicleId",
                table: "vehicleUsageSnapshots",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
