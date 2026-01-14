using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaziLease.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialFieldsToMaintenance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BillableAmount",
                table: "VehicleMaintenance",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceClaimNumber",
                table: "VehicleMaintenance",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InsuranceClaimStatus",
                table: "VehicleMaintenance",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceName",
                table: "VehicleMaintenance",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBillableToClient",
                table: "VehicleMaintenance",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VehicleLeaseId",
                table: "VehicleMaintenance",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_VehicleMaintenance_VehicleLeaseId",
                table: "VehicleMaintenance",
                column: "VehicleLeaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleMaintenance_VehicleLeases_VehicleLeaseId",
                table: "VehicleMaintenance",
                column: "VehicleLeaseId",
                principalTable: "VehicleLeases",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleMaintenance_VehicleLeases_VehicleLeaseId",
                table: "VehicleMaintenance");

            migrationBuilder.DropIndex(
                name: "IX_VehicleMaintenance_VehicleLeaseId",
                table: "VehicleMaintenance");

            migrationBuilder.DropColumn(
                name: "BillableAmount",
                table: "VehicleMaintenance");

            migrationBuilder.DropColumn(
                name: "InsuranceClaimNumber",
                table: "VehicleMaintenance");

            migrationBuilder.DropColumn(
                name: "InsuranceClaimStatus",
                table: "VehicleMaintenance");

            migrationBuilder.DropColumn(
                name: "InsuranceName",
                table: "VehicleMaintenance");

            migrationBuilder.DropColumn(
                name: "IsBillableToClient",
                table: "VehicleMaintenance");

            migrationBuilder.DropColumn(
                name: "VehicleLeaseId",
                table: "VehicleMaintenance");
        }
    }
}
