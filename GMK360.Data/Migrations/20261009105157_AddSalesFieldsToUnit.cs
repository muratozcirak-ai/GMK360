using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesFieldsToUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedAgent",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ListPrice",
                table: "BuildingUnits",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ListingType",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesStage",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SalesStatus",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedAgent",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "ListPrice",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "ListingType",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "SalesStage",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "SalesStatus",
                table: "BuildingUnits");
        }
    }
}
