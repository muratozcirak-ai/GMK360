using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmFieldsToSpaceFixture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "SpaceFixtures",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TechnicalServiceId",
                table: "SpaceFixtures",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicalServiceName",
                table: "SpaceFixtures",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpaceFixtures_SupplierId",
                table: "SpaceFixtures",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SpaceFixtures_TechnicalServiceId",
                table: "SpaceFixtures",
                column: "TechnicalServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_SpaceFixtures_ServiceProviders_SupplierId",
                table: "SpaceFixtures",
                column: "SupplierId",
                principalTable: "ServiceProviders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SpaceFixtures_ServiceProviders_TechnicalServiceId",
                table: "SpaceFixtures",
                column: "TechnicalServiceId",
                principalTable: "ServiceProviders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SpaceFixtures_ServiceProviders_SupplierId",
                table: "SpaceFixtures");

            migrationBuilder.DropForeignKey(
                name: "FK_SpaceFixtures_ServiceProviders_TechnicalServiceId",
                table: "SpaceFixtures");

            migrationBuilder.DropIndex(
                name: "IX_SpaceFixtures_SupplierId",
                table: "SpaceFixtures");

            migrationBuilder.DropIndex(
                name: "IX_SpaceFixtures_TechnicalServiceId",
                table: "SpaceFixtures");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "SpaceFixtures");

            migrationBuilder.DropColumn(
                name: "TechnicalServiceId",
                table: "SpaceFixtures");

            migrationBuilder.DropColumn(
                name: "TechnicalServiceName",
                table: "SpaceFixtures");
        }
    }
}
