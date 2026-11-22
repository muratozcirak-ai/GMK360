using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerTenantPinsAndExpenseGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerQuickPinHash",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantQuickPinHash",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExpenseGroup",
                table: "BuildingFixedExpenses",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExpenseGroup",
                table: "BuildingExpenses",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerQuickPinHash",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "TenantQuickPinHash",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "ExpenseGroup",
                table: "BuildingFixedExpenses");

            migrationBuilder.DropColumn(
                name: "ExpenseGroup",
                table: "BuildingExpenses");
        }
    }
}
