using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaziLease.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleLeaseTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLease_Clients_ClientId",
                table: "VehicleLease");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLease_Vehicles_VehicleId",
                table: "VehicleLease");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_VehicleLease_CurrentLeaseId",
                table: "Vehicles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleLease",
                table: "VehicleLease");

            migrationBuilder.RenameTable(
                name: "VehicleLease",
                newName: "VehicleLeases");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleLease_VehicleId",
                table: "VehicleLeases",
                newName: "IX_VehicleLeases_VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleLease_ClientId",
                table: "VehicleLeases",
                newName: "IX_VehicleLeases_ClientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleLeases",
                table: "VehicleLeases",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLeases_Clients_ClientId",
                table: "VehicleLeases",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLeases_Vehicles_VehicleId",
                table: "VehicleLeases",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_VehicleLeases_CurrentLeaseId",
                table: "Vehicles",
                column: "CurrentLeaseId",
                principalTable: "VehicleLeases",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLeases_Clients_ClientId",
                table: "VehicleLeases");

            migrationBuilder.DropForeignKey(
                name: "FK_VehicleLeases_Vehicles_VehicleId",
                table: "VehicleLeases");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_VehicleLeases_CurrentLeaseId",
                table: "Vehicles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_VehicleLeases",
                table: "VehicleLeases");

            migrationBuilder.RenameTable(
                name: "VehicleLeases",
                newName: "VehicleLease");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleLeases_VehicleId",
                table: "VehicleLease",
                newName: "IX_VehicleLease_VehicleId");

            migrationBuilder.RenameIndex(
                name: "IX_VehicleLeases_ClientId",
                table: "VehicleLease",
                newName: "IX_VehicleLease_ClientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_VehicleLease",
                table: "VehicleLease",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLease_Clients_ClientId",
                table: "VehicleLease",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleLease_Vehicles_VehicleId",
                table: "VehicleLease",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_VehicleLease_CurrentLeaseId",
                table: "Vehicles",
                column: "CurrentLeaseId",
                principalTable: "VehicleLease",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
