using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEstimatedBudgetToPhaseTask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedBudget",
                table: "PhaseTasks",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedQuantity",
                table: "PhaseTasks",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "PhaseTasks",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedBudget",
                table: "PhaseTasks");

            migrationBuilder.DropColumn(
                name: "EstimatedQuantity",
                table: "PhaseTasks");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "PhaseTasks");
        }
    }
}
