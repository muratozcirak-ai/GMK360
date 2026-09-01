using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOccupantTypeAndTaxes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OccupantType",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PropertyTaxBaseValue",
                table: "Properties",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RentAmount",
                table: "Properties",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OccupantType",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "PropertyTaxBaseValue",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "RentAmount",
                table: "Properties");
        }
    }
}
