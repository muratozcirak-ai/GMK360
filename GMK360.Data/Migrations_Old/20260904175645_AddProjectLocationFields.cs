using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectLocationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "ConstructionProjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "ConstructionProjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NeighborhoodId",
                table: "ConstructionProjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StreetId",
                table: "ConstructionProjects",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CityId",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "NeighborhoodId",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "StreetId",
                table: "ConstructionProjects");
        }
    }
}
