using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyCashTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DailySgkCost",
                table: "AgencyWorkers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetDailyWage",
                table: "AgencyWorkers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "AgencyConsultantId",
                table: "AgencyStaffAdvances",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AgencyWorkerId",
                table: "AgencyStaffAdvances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlySgkCost",
                table: "AgencyConsultants",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "AgencyCashTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyId = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    SupplierCurrentAccountId = table.Column<int>(type: "int", nullable: true),
                    AgencyWorkerId = table.Column<int>(type: "int", nullable: true),
                    AgencyConsultantId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HandledByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyCashTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgencyCashTransactions_Agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AgencyCashTransactions_AgencyConsultants_AgencyConsultantId",
                        column: x => x.AgencyConsultantId,
                        principalTable: "AgencyConsultants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AgencyCashTransactions_AgencyWorkers_AgencyWorkerId",
                        column: x => x.AgencyWorkerId,
                        principalTable: "AgencyWorkers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AgencyCashTransactions_SupplierCurrentAccounts_SupplierCurrentAccountId",
                        column: x => x.SupplierCurrentAccountId,
                        principalTable: "SupplierCurrentAccounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgencyStaffAdvances_AgencyWorkerId",
                table: "AgencyStaffAdvances",
                column: "AgencyWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyCashTransactions_AgencyConsultantId",
                table: "AgencyCashTransactions",
                column: "AgencyConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyCashTransactions_AgencyId",
                table: "AgencyCashTransactions",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyCashTransactions_AgencyWorkerId",
                table: "AgencyCashTransactions",
                column: "AgencyWorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyCashTransactions_SupplierCurrentAccountId",
                table: "AgencyCashTransactions",
                column: "SupplierCurrentAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_AgencyStaffAdvances_AgencyWorkers_AgencyWorkerId",
                table: "AgencyStaffAdvances",
                column: "AgencyWorkerId",
                principalTable: "AgencyWorkers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgencyStaffAdvances_AgencyWorkers_AgencyWorkerId",
                table: "AgencyStaffAdvances");

            migrationBuilder.DropTable(
                name: "AgencyCashTransactions");

            migrationBuilder.DropIndex(
                name: "IX_AgencyStaffAdvances_AgencyWorkerId",
                table: "AgencyStaffAdvances");

            migrationBuilder.DropColumn(
                name: "DailySgkCost",
                table: "AgencyWorkers");

            migrationBuilder.DropColumn(
                name: "NetDailyWage",
                table: "AgencyWorkers");

            migrationBuilder.DropColumn(
                name: "AgencyWorkerId",
                table: "AgencyStaffAdvances");

            migrationBuilder.DropColumn(
                name: "MonthlySgkCost",
                table: "AgencyConsultants");

            migrationBuilder.AlterColumn<int>(
                name: "AgencyConsultantId",
                table: "AgencyStaffAdvances",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
