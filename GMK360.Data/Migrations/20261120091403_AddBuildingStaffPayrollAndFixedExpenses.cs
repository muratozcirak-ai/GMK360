using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBuildingStaffPayrollAndFixedExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BuildingFixedExpenses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ExpenseScope = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DistributionRule = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DueDayOfMonth = table.Column<int>(type: "int", nullable: false),
                    ProviderName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SubscriberNo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MonthlyAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsAutoRecurring = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingFixedExpenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildingFixedExpenses_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BuildingStaffs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TcIdentityNo = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    SgkNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RoleType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NetSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SgkEmployerShare = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IncomeAndStampTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FringeBenefits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlySeveranceFund = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeaveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingStaffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildingStaffs_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BuildingStaffLeaves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingStaffId = table.Column<int>(type: "int", nullable: false),
                    LeaveYear = table.Column<int>(type: "int", nullable: false),
                    LeaveType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalDays = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReplacementCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingStaffLeaves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildingStaffLeaves_BuildingStaffs_BuildingStaffId",
                        column: x => x.BuildingStaffId,
                        principalTable: "BuildingStaffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingStaffMonthlyPayrolls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingStaffId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    BaseNetSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BaseSgkEmployerShare = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BaseIncomeAndStampTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BaseFringeBenefits = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlySeveranceFund = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HolidayDaysWorked = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    UnpaidLeaveDays = table.Column<int>(type: "int", nullable: false),
                    ReplacementStaffCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsSalaryPaid = table.Column<bool>(type: "bit", nullable: false),
                    SalaryPaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsSgkPaid = table.Column<bool>(type: "bit", nullable: false),
                    SgkPaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidFromBankAccountId = table.Column<int>(type: "int", nullable: true),
                    OperatingExpenseId = table.Column<int>(type: "int", nullable: true),
                    SeveranceExpenseId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingStaffMonthlyPayrolls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildingStaffMonthlyPayrolls_BuildingStaffs_BuildingStaffId",
                        column: x => x.BuildingStaffId,
                        principalTable: "BuildingStaffs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuildingFixedExpenses_BuildingId",
                table: "BuildingFixedExpenses",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingStaffLeaves_BuildingStaffId",
                table: "BuildingStaffLeaves",
                column: "BuildingStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingStaffMonthlyPayrolls_BuildingStaffId",
                table: "BuildingStaffMonthlyPayrolls",
                column: "BuildingStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingStaffs_BuildingId",
                table: "BuildingStaffs",
                column: "BuildingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BuildingFixedExpenses");

            migrationBuilder.DropTable(
                name: "BuildingStaffLeaves");

            migrationBuilder.DropTable(
                name: "BuildingStaffMonthlyPayrolls");

            migrationBuilder.DropTable(
                name: "BuildingStaffs");
        }
    }
}
