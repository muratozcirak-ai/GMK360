using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSpaceMeasurementDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Height",
                table: "SpaceMeasurements",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Length",
                table: "SpaceMeasurements",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Width",
                table: "SpaceMeasurements",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "SpaceMeasurements");

            migrationBuilder.DropColumn(
                name: "Length",
                table: "SpaceMeasurements");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "SpaceMeasurements");
        }
    }
}
