using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EaziLease.Migrations
{
    /// <inheritdoc />
    public partial class RelateClientToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_CompanyName",
                table: "Clients");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CompanyName",
                table: "Clients",
                column: "CompanyName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_CompanyName",
                table: "Clients");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_CompanyName",
                table: "Clients",
                column: "CompanyName",
                unique: true);
        }
    }
}
