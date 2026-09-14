using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyPayrollSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AgencyConsultants",
                table: "AgencyConsultants");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "AgencyConsultants");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "AgencyConsultants",
                type: "int",
                nullable: false)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "AgencyConsultants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdentityNumber",
                table: "AgencyConsultants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlySalary",
                table: "AgencyConsultants",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AgencyConsultants",
                table: "AgencyConsultants",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "AgencyStaffPayrolls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyConsultantId = table.Column<int>(type: "int", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GenerationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BaseSalary = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BonusAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeductionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPayableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyStaffPayrolls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgencyStaffPayrolls_AgencyConsultants_AgencyConsultantId",
                        column: x => x.AgencyConsultantId,
                        principalTable: "AgencyConsultants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AgencyStaffAdvances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyConsultantId = table.Column<int>(type: "int", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StatusDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeductedFromPayrollId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyStaffAdvances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgencyStaffAdvances_AgencyConsultants_AgencyConsultantId",
                        column: x => x.AgencyConsultantId,
                        principalTable: "AgencyConsultants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgencyStaffAdvances_AgencyStaffPayrolls_DeductedFromPayrollId",
                        column: x => x.DeductedFromPayrollId,
                        principalTable: "AgencyStaffPayrolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgencyConsultants_AgencyId_UserId",
                table: "AgencyConsultants",
                columns: new[] { "AgencyId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgencyStaffAdvances_AgencyConsultantId",
                table: "AgencyStaffAdvances",
                column: "AgencyConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyStaffAdvances_DeductedFromPayrollId",
                table: "AgencyStaffAdvances",
                column: "DeductedFromPayrollId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyStaffPayrolls_AgencyConsultantId",
                table: "AgencyStaffPayrolls",
                column: "AgencyConsultantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgencyStaffAdvances");

            migrationBuilder.DropTable(
                name: "AgencyStaffPayrolls");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AgencyConsultants",
                table: "AgencyConsultants");

            migrationBuilder.DropIndex(
                name: "IX_AgencyConsultants_AgencyId_UserId",
                table: "AgencyConsultants");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "AgencyConsultants");

            migrationBuilder.DropColumn(
                name: "IdentityNumber",
                table: "AgencyConsultants");

            migrationBuilder.DropColumn(
                name: "MonthlySalary",
                table: "AgencyConsultants");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "AgencyConsultants",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AgencyConsultants",
                table: "AgencyConsultants",
                columns: new[] { "AgencyId", "UserId" });
        }
    }
}
