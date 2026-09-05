using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectLifecycleStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LifecycleStatus",
                table: "ConstructionProjects",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LifecycleStatus",
                table: "ConstructionProjects");
        }
    }
}
