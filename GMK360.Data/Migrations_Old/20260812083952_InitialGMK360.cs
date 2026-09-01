using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialGMK360 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Classification",
                table: "BuildingUnits");

            migrationBuilder.AddColumn<int>(
                name: "UnitTypeId",
                table: "BuildingUnits",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_UnitTypeId",
                table: "BuildingUnits",
                column: "UnitTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingUnits_DefinitionValues_UnitTypeId",
                table: "BuildingUnits",
                column: "UnitTypeId",
                principalTable: "DefinitionValues",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildingUnits_DefinitionValues_UnitTypeId",
                table: "BuildingUnits");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUnits_UnitTypeId",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "UnitTypeId",
                table: "BuildingUnits");

            migrationBuilder.AddColumn<int>(
                name: "Classification",
                table: "BuildingUnits",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
