using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase9_FinanceEscrowTax : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EscrowTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentTransactionId = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlatformCommissionGross = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SellerUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SellerGrossAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SellerTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SellerNetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReferrerUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReferrerGrossAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReferrerTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReferrerNetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EscrowTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EscrowTransactions_AspNetUsers_ReferrerUserId",
                        column: x => x.ReferrerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscrowTransactions_AspNetUsers_SellerUserId",
                        column: x => x.SellerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EscrowTransactions_PaymentTransactions_PaymentTransactionId",
                        column: x => x.PaymentTransactionId,
                        principalTable: "PaymentTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GlobalFinanceSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DefaultWithholdingTaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    EscrowHoldDays = table.Column<int>(type: "int", nullable: false),
                    MinimumWithdrawalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlobalFinanceSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemFunds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemFunds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserFinancialProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsCorporate = table.Column<bool>(type: "bit", nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TaxOffice = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IBAN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CustomWithholdingTaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFinancialProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFinancialProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemFundTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SystemFundId = table.Column<int>(type: "int", nullable: false),
                    SourceEscrowTransactionId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemFundTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemFundTransactions_EscrowTransactions_SourceEscrowTransactionId",
                        column: x => x.SourceEscrowTransactionId,
                        principalTable: "EscrowTransactions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SystemFundTransactions_SystemFunds_SystemFundId",
                        column: x => x.SystemFundId,
                        principalTable: "SystemFunds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EscrowTransactions_PaymentTransactionId",
                table: "EscrowTransactions",
                column: "PaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowTransactions_ReferrerUserId",
                table: "EscrowTransactions",
                column: "ReferrerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EscrowTransactions_SellerUserId",
                table: "EscrowTransactions",
                column: "SellerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemFundTransactions_SourceEscrowTransactionId",
                table: "SystemFundTransactions",
                column: "SourceEscrowTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemFundTransactions_SystemFundId",
                table: "SystemFundTransactions",
                column: "SystemFundId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFinancialProfiles_UserId",
                table: "UserFinancialProfiles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GlobalFinanceSettings");

            migrationBuilder.DropTable(
                name: "SystemFundTransactions");

            migrationBuilder.DropTable(
                name: "UserFinancialProfiles");

            migrationBuilder.DropTable(
                name: "EscrowTransactions");

            migrationBuilder.DropTable(
                name: "SystemFunds");
        }
    }
}
