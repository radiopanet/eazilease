using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaziLease.Migrations
{
    /// <inheritdoc />
    public partial class FixFinanceFieldNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaseFinacialSummaries_VehicleLeases_LeaseId",
                table: "LeaseFinacialSummaries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LeaseFinacialSummaries",
                table: "LeaseFinacialSummaries");

            migrationBuilder.RenameTable(
                name: "LeaseFinacialSummaries",
                newName: "LeaseFinancialSummaries");

            migrationBuilder.RenameColumn(
                name: "TotalBillableMaitenance",
                table: "LeaseFinancialSummaries",
                newName: "TotalBillableMaintenance");

            migrationBuilder.RenameColumn(
                name: "CalculateBy",
                table: "LeaseFinancialSummaries",
                newName: "CalculatedBy");

            migrationBuilder.RenameIndex(
                name: "IX_LeaseFinacialSummaries_LeaseId",
                table: "LeaseFinancialSummaries",
                newName: "IX_LeaseFinancialSummaries_LeaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeaseFinancialSummaries",
                table: "LeaseFinancialSummaries",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaseFinancialSummaries_VehicleLeases_LeaseId",
                table: "LeaseFinancialSummaries",
                column: "LeaseId",
                principalTable: "VehicleLeases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaseFinancialSummaries_VehicleLeases_LeaseId",
                table: "LeaseFinancialSummaries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LeaseFinancialSummaries",
                table: "LeaseFinancialSummaries");

            migrationBuilder.RenameTable(
                name: "LeaseFinancialSummaries",
                newName: "LeaseFinacialSummaries");

            migrationBuilder.RenameColumn(
                name: "TotalBillableMaintenance",
                table: "LeaseFinacialSummaries",
                newName: "TotalBillableMaitenance");

            migrationBuilder.RenameColumn(
                name: "CalculatedBy",
                table: "LeaseFinacialSummaries",
                newName: "CalculateBy");

            migrationBuilder.RenameIndex(
                name: "IX_LeaseFinancialSummaries_LeaseId",
                table: "LeaseFinacialSummaries",
                newName: "IX_LeaseFinacialSummaries_LeaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LeaseFinacialSummaries",
                table: "LeaseFinacialSummaries",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaseFinacialSummaries_VehicleLeases_LeaseId",
                table: "LeaseFinacialSummaries",
                column: "LeaseId",
                principalTable: "VehicleLeases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
