using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialObligationTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNotaryApproved",
                table: "TenancyContracts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "NotaryName",
                table: "TenancyContracts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NotaryNumber",
                table: "TenancyContracts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PayerType",
                table: "BuildingExpenses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "BuildingExpenses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "ExpenseType",
                table: "BuildingExpenses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceDocumentUrl",
                table: "BuildingExpenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "BuildingExpenses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "BuildingExpenses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BuildingExpenseShares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingExpenseId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    ShareAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ResponsibleUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingExpenseShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildingExpenseShares_BuildingExpenses_BuildingExpenseId",
                        column: x => x.BuildingExpenseId,
                        principalTable: "BuildingExpenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuildingExpenseShares_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialObligationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TargetPropertyType = table.Column<int>(type: "int", nullable: false),
                    ResponsibleRole = table.Column<int>(type: "int", nullable: false),
                    PaymentFrequency = table.Column<int>(type: "int", nullable: false),
                    FirstInstallmentMonth = table.Column<int>(type: "int", nullable: true),
                    SecondInstallmentMonth = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialObligationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncomeTaxDeclarations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxYear = table.Column<int>(type: "int", nullable: false),
                    TotalRentalIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LegalExemptionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalWithholdingTaxPaidByTenants = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CalculatedTaxBase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeductionMethod = table.Column<int>(type: "int", nullable: false),
                    DeclarationStatus = table.Column<int>(type: "int", nullable: false),
                    DocumentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeTaxDeclarations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyExpenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    ExpenseCategory = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceDocumentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsTaxDeductible = table.Column<bool>(type: "bit", nullable: false),
                    TaxYear = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyExpenses_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaxParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxYear = table.Column<int>(type: "int", nullable: false),
                    ResidentialExemptionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommercialExemptionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxBracketsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxParameters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyFinancialSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ObligationTypeId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsNotificationSent = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyFinancialSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyFinancialSchedules_FinancialObligationTypes_ObligationTypeId",
                        column: x => x.ObligationTypeId,
                        principalTable: "FinancialObligationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyFinancialSchedules_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "FinancialObligationTypes",
                columns: new[] { "Id", "CreatedAt", "FirstInstallmentMonth", "IsActive", "IsDeleted", "Name", "PaymentFrequency", "ResponsibleRole", "SecondInstallmentMonth", "TargetPropertyType", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, true, false, "Emlak Vergisi", 3, 1, 11, 3, null },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, true, false, "Kira Gelir Vergisi (GMSİ)", 3, 1, 7, 3, null },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, true, false, "Çevre Temizlik Vergisi (ÇTV)", 3, 2, 11, 3, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingExpenseShares_BuildingExpenseId",
                table: "BuildingExpenseShares",
                column: "BuildingExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingExpenseShares_PropertyId",
                table: "BuildingExpenseShares",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyExpenses_PropertyId",
                table: "PropertyExpenses",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyFinancialSchedules_ObligationTypeId",
                table: "PropertyFinancialSchedules",
                column: "ObligationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyFinancialSchedules_PropertyId",
                table: "PropertyFinancialSchedules",
                column: "PropertyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingExpenseShares");

            migrationBuilder.DropTable(
                name: "IncomeTaxDeclarations");

            migrationBuilder.DropTable(
                name: "PropertyExpenses");

            migrationBuilder.DropTable(
                name: "PropertyFinancialSchedules");

            migrationBuilder.DropTable(
                name: "TaxParameters");

            migrationBuilder.DropTable(
                name: "FinancialObligationTypes");

            migrationBuilder.DropColumn(
                name: "IsNotaryApproved",
                table: "TenancyContracts");

            migrationBuilder.DropColumn(
                name: "NotaryName",
                table: "TenancyContracts");

            migrationBuilder.DropColumn(
                name: "NotaryNumber",
                table: "TenancyContracts");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "BuildingExpenses");

            migrationBuilder.DropColumn(
                name: "ExpenseType",
                table: "BuildingExpenses");

            migrationBuilder.DropColumn(
                name: "InvoiceDocumentUrl",
                table: "BuildingExpenses");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "BuildingExpenses");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "BuildingExpenses");

            migrationBuilder.AlterColumn<string>(
                name: "PayerType",
                table: "BuildingExpenses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
