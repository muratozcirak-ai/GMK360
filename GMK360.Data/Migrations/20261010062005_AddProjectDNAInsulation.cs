using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectDNAInsulation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FacadeInsulation",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FoundationInsulation",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoofMaterial",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacadeInsulation",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "FoundationInsulation",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "RoofMaterial",
                table: "ConstructionProjects");
        }
    }
}
