using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddContractArchiveAndBillingCycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BillingCycleMonths",
                table: "BuildingFixedExpenses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BillingMonth",
                table: "BuildingFixedExpenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractDocumentUrl",
                table: "BuildingFixedExpenses",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDate",
                table: "BuildingFixedExpenses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhysicalArchiveLocation",
                table: "BuildingFixedExpenses",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingCycleMonths",
                table: "BuildingFixedExpenses");

            migrationBuilder.DropColumn(
                name: "BillingMonth",
                table: "BuildingFixedExpenses");

            migrationBuilder.DropColumn(
                name: "ContractDocumentUrl",
                table: "BuildingFixedExpenses");

            migrationBuilder.DropColumn(
                name: "ContractEndDate",
                table: "BuildingFixedExpenses");

            migrationBuilder.DropColumn(
                name: "PhysicalArchiveLocation",
                table: "BuildingFixedExpenses");
        }
    }
}
