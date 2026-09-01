using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddZipCodeToNeighborhood : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ZipCode",
                table: "Neighborhoods",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 1,
                column: "ZipCode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 2,
                column: "ZipCode",
                value: null);

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 3,
                column: "ZipCode",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZipCode",
                table: "Neighborhoods");
        }
    }
}
