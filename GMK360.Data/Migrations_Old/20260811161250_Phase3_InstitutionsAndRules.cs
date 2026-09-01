using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_InstitutionsAndRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExpenseCategoryId",
                table: "BuildingExpenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstitutionId",
                table: "BuildingExpenses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExpenseCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsTaxDeductible = table.Column<bool>(type: "bit", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Institutions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NormalizedCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InstitutionType = table.Column<int>(type: "int", nullable: false),
                    IsApiSupported = table.Column<bool>(type: "bit", nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Institutions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GlobalObligationRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Rate = table.Column<decimal>(type: "decimal(5,4)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TargetExpenseCategoryId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalObligationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlobalObligationRules_ExpenseCategories_TargetExpenseCategoryId",
                        column: x => x.TargetExpenseCategoryId,
                        principalTable: "ExpenseCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Institutions",
                columns: new[] { "Id", "CreatedAt", "InstitutionType", "IsApiSupported", "IsDeleted", "LogoUrl", "Name", "NormalizedCode", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, false, null, "TEDAŞ", "tedas", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, true, false, null, "İGDAŞ", "igdas", null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, false, false, null, "İSKİ", "iski", null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, false, false, null, "ASAT", "asat", null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, false, null, "ENERJİSA", "enerjisa", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingExpenses_ExpenseCategoryId",
                table: "BuildingExpenses",
                column: "ExpenseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingExpenses_InstitutionId",
                table: "BuildingExpenses",
                column: "InstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_GlobalObligationRules_TargetExpenseCategoryId",
                table: "GlobalObligationRules",
                column: "TargetExpenseCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingExpenses_ExpenseCategories_ExpenseCategoryId",
                table: "BuildingExpenses",
                column: "ExpenseCategoryId",
                principalTable: "ExpenseCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingExpenses_Institutions_InstitutionId",
                table: "BuildingExpenses",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildingExpenses_ExpenseCategories_ExpenseCategoryId",
                table: "BuildingExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_BuildingExpenses_Institutions_InstitutionId",
                table: "BuildingExpenses");

            migrationBuilder.DropTable(
                name: "GlobalObligationRules");

            migrationBuilder.DropTable(
                name: "Institutions");

            migrationBuilder.DropTable(
                name: "ExpenseCategories");

            migrationBuilder.DropIndex(
                name: "IX_BuildingExpenses_ExpenseCategoryId",
                table: "BuildingExpenses");

            migrationBuilder.DropIndex(
                name: "IX_BuildingExpenses_InstitutionId",
                table: "BuildingExpenses");

            migrationBuilder.DropColumn(
                name: "ExpenseCategoryId",
                table: "BuildingExpenses");

            migrationBuilder.DropColumn(
                name: "InstitutionId",
                table: "BuildingExpenses");
        }
    }
}
