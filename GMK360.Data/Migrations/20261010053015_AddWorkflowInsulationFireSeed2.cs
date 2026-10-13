using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowInsulationFireSeed2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "StepOrder" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 10 });

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "StepOrder" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 11 });

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "StepOrder" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 12 });

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "StepOrder" },
                values: new object[] { new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 13 });

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.InsertData(
                table: "WorkflowTemplates",
                columns: new[] { "Id", "CreatedAt", "ExperienceNote", "IsDeleted", "IsGatekeeper", "PhaseCategory", "StepOrder", "TaskName", "UpdatedAt", "WaitDays", "WorkflowType" },
                values: new object[,]
                {
                    { 21, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "DİKKAT: İtfaiye onayı için yangın dolabı ve hortum tesisatı standartlara uygun montajlanmalı!", false, true, "TipKat", 8, "İtfaiye ve Yangın Tesisatı (Hortum/Dolap)", null, 0, "InceSabit" },
                    { 22, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Banyo ve tuvaletlerde su izolasyonu yapılıp 24 saat su testi (göllendirme) yapılmadan şap atılmayacak.", false, true, "TipKat", 9, "Islak Hacim Su Yalıtımı", null, 1, "InceSabit" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(8355));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9800));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9802));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9804));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9826));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 6,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9828));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 7,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9829));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 8,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9831));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 9,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9832));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 10,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9835));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 11,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9837));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 12,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9838));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 13,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9840));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 14,
                columns: new[] { "CreatedAt", "StepOrder" },
                values: new object[] { new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9842), 8 });

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 15,
                columns: new[] { "CreatedAt", "StepOrder" },
                values: new object[] { new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9844), 9 });

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 16,
                columns: new[] { "CreatedAt", "StepOrder" },
                values: new object[] { new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9845), 10 });

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 17,
                columns: new[] { "CreatedAt", "StepOrder" },
                values: new object[] { new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9847), 11 });

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 18,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9848));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 19,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9850));

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 20,
                column: "CreatedAt",
                value: new DateTime(2026, 10, 9, 15, 53, 44, 81, DateTimeKind.Utc).AddTicks(9851));
        }
    }
}
