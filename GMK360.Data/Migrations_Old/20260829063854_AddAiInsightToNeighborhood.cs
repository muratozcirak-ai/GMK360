using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAiInsightToNeighborhood : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiAnalysisReport",
                table: "Neighborhoods",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AiAnalysisUpdatedAt",
                table: "Neighborhoods",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "AiAnalysisReport", "AiAnalysisUpdatedAt" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "AiAnalysisReport", "AiAnalysisUpdatedAt" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "AiAnalysisReport", "AiAnalysisUpdatedAt" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiAnalysisReport",
                table: "Neighborhoods");

            migrationBuilder.DropColumn(
                name: "AiAnalysisUpdatedAt",
                table: "Neighborhoods");
        }
    }
}
