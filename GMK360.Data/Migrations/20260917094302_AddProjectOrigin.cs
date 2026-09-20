using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectOrigin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectOriginId",
                table: "ConstructionProjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectOwnerContact",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProjectOriginId",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "ProjectOwnerContact",
                table: "ConstructionProjects");
        }
    }
}
