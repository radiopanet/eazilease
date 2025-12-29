using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaziLease.Migrations
{
    /// <inheritdoc />
    public partial class FixVehicleDriverRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Vehicles_CurrentVehicleId",
                table: "Drivers");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Vehicles_CurrentVehicleId",
                table: "Drivers",
                column: "CurrentVehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drivers_Vehicles_CurrentVehicleId",
                table: "Drivers");

            migrationBuilder.AddForeignKey(
                name: "FK_Drivers_Vehicles_CurrentVehicleId",
                table: "Drivers",
                column: "CurrentVehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id");
        }
    }
}
