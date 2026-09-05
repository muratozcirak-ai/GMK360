using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConstructionTargets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetTotalApartments",
                table: "ConstructionProjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetTotalShops",
                table: "ConstructionProjects",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TargetTotalApartments",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "TargetTotalShops",
                table: "ConstructionProjects");
        }
    }
}
