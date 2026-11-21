using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDuesRulesAndUnitExemptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CleaningParticipationPercent",
                table: "BuildingUnits",
                type: "decimal(6,2)",
                nullable: false,
                defaultValue: 100m);

            migrationBuilder.AddColumn<decimal>(
                name: "DuesMultiplier",
                table: "BuildingUnits",
                type: "decimal(8,2)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<string>(
                name: "DuesRuleNote",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedMonthlyDues",
                table: "BuildingUnits",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExemptFromElevator",
                table: "BuildingUnits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsExemptFromStairwell",
                table: "BuildingUnits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ExpenseItemKey",
                table: "BuildingFixedExpenses",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Genel");

            migrationBuilder.AddColumn<decimal>(
                name: "OwnerSplitPercent",
                table: "BuildingFixedExpenses",
                type: "decimal(6,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScopeType",
                table: "BuildingFixedExpenses",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<decimal>(
                name: "ShopParticipationPercent",
                table: "BuildingFixedExpenses",
                type: "decimal(6,2)",
                nullable: false,
                defaultValue: 100m);

            migrationBuilder.AddColumn<string>(
                name: "DistributionRule",
                table: "BuildingExpenses",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Esit");

            migrationBuilder.AddColumn<string>(
                name: "ExpenseItemKey",
                table: "BuildingExpenses",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "Genel");

            migrationBuilder.AddColumn<decimal>(
                name: "OwnerSplitPercent",
                table: "BuildingExpenses",
                type: "decimal(6,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShopParticipationPercent",
                table: "BuildingExpenses",
                type: "decimal(6,2)",
                nullable: false,
                defaultValue: 100m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CleaningParticipationPercent",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "DuesMultiplier",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "DuesRuleNote",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "FixedMonthlyDues",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "IsExemptFromElevator",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "IsExemptFromStairwell",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "ExpenseItemKey",
                table: "BuildingFixedExpenses");

            migrationBuilder.DropColumn(
                name: "OwnerSplitPercent",
                table: "BuildingFixedExpenses");

            migrationBuilder.DropColumn(
                name: "ScopeType",
                table: "BuildingFixedExpenses");

            migrationBuilder.DropColumn(
                name: "ShopParticipationPercent",
                table: "BuildingFixedExpenses");

            migrationBuilder.DropColumn(
                name: "DistributionRule",
                table: "BuildingExpenses");

            migrationBuilder.DropColumn(
                name: "ExpenseItemKey",
                table: "BuildingExpenses");

            migrationBuilder.DropColumn(
                name: "OwnerSplitPercent",
                table: "BuildingExpenses");

            migrationBuilder.DropColumn(
                name: "ShopParticipationPercent",
                table: "BuildingExpenses");
        }
    }
}
