using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBlockSpecificFeatures2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ElevatorCount",
                table: "Buildings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsulationType",
                table: "Buildings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParkingType",
                table: "Buildings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ElevatorCount",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "InsulationType",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "ParkingType",
                table: "Buildings");
        }
    }
}
