using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCommercialTaxes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "FinancialObligationTypes",
                columns: new[] { "Id", "CreatedAt", "FirstInstallmentMonth", "IsActive", "IsDeleted", "Name", "PaymentFrequency", "ResponsibleRole", "SecondInstallmentMonth", "TargetPropertyType", "UpdatedAt" },
                values: new object[,]
                {
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 5, true, false, "İlan ve Reklam Vergisi (Tabela)", 3, 2, 11, 2, null },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, "Kira Stopajı (Muhtasar Beyanname)", 1, 2, null, 2, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 5);
        }
    }
}
