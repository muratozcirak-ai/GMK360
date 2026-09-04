using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FacadeDirection",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "GrossSquareMeters",
                table: "BuildingUnits",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "NetSquareMeters",
                table: "BuildingUnits",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacadeDirection",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "GrossSquareMeters",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "NetSquareMeters",
                table: "BuildingUnits");
        }
    }
}
