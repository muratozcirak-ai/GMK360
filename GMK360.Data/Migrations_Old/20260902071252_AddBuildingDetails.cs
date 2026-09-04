using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Buildings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicalFeatures",
                table: "Buildings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "TechnicalFeatures",
                table: "Buildings");
        }
    }
}
