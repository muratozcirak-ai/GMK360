using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseDuesAndUnitExemptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DuesAdjustmentAmount",
                table: "BuildingUnits",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsDuesExempt",
                table: "BuildingUnits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultBaseMonthlyDues",
                table: "Buildings",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DuesPaymentDay",
                table: "Buildings",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DuesAdjustmentAmount",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "IsDuesExempt",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "DefaultBaseMonthlyDues",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "DuesPaymentDay",
                table: "Buildings");
        }
    }
}
