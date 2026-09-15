using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExpandSupplierTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "SupplierAccountTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierAccountTransactions_ProjectId",
                table: "SupplierAccountTransactions",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierAccountTransactions_ConstructionProjects_ProjectId",
                table: "SupplierAccountTransactions",
                column: "ProjectId",
                principalTable: "ConstructionProjects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplierAccountTransactions_ConstructionProjects_ProjectId",
                table: "SupplierAccountTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SupplierAccountTransactions_ProjectId",
                table: "SupplierAccountTransactions");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "SupplierAccountTransactions");
        }
    }
}
