using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentStateImageUrl",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "ConstructionProjects",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "ConstructionProjects",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentStateImageUrl",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "ConstructionProjects");
        }
    }
}
