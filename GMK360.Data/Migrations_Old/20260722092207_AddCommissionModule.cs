using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCommissionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CommissionAmount",
                table: "PropertyReservations",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "CommissionDocumentType",
                table: "PropertyReservations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CommissionPaymentDate",
                table: "PropertyReservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCommissionPaid",
                table: "PropertyReservations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                table: "PropertyReservations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    FullNameOrCompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsTaxpayer = table.Column<bool>(type: "bit", nullable: false),
                    TaxNumberOrTc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyReservations_PartnerId",
                table: "PropertyReservations",
                column: "PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PropertyReservations_Partners_PartnerId",
                table: "PropertyReservations",
                column: "PartnerId",
                principalTable: "Partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PropertyReservations_Partners_PartnerId",
                table: "PropertyReservations");

            migrationBuilder.DropTable(
                name: "Partners");

            migrationBuilder.DropIndex(
                name: "IX_PropertyReservations_PartnerId",
                table: "PropertyReservations");

            migrationBuilder.DropColumn(
                name: "CommissionAmount",
                table: "PropertyReservations");

            migrationBuilder.DropColumn(
                name: "CommissionDocumentType",
                table: "PropertyReservations");

            migrationBuilder.DropColumn(
                name: "CommissionPaymentDate",
                table: "PropertyReservations");

            migrationBuilder.DropColumn(
                name: "IsCommissionPaid",
                table: "PropertyReservations");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "PropertyReservations");
        }
    }
}
