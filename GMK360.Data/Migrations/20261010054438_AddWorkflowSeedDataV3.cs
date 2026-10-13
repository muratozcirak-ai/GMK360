using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowSeedDataV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MaterialRecipes",
                columns: new[] { "Id", "BaseUnit", "CreatedAt", "IsDeleted", "UpdatedAt", "WorkItemCode", "WorkItemName" },
                values: new object[,]
                {
                    { 1, "m2", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, "DUVAR_YTONG_20", "20'lik Ytong Duvar Örümü" },
                    { 2, "m2", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, null, "DUVAR_TUGLA_13", "13.5'luk Harman Tuğla Örümü" }
                });

            migrationBuilder.InsertData(
                table: "WorkflowTemplates",
                columns: new[] { "Id", "CreatedAt", "ExperienceNote", "IsDeleted", "IsGatekeeper", "PhaseCategory", "RequiredCondition", "StepOrder", "TaskName", "UpdatedAt", "WaitDays", "WorkflowType" },
                values: new object[,]
                {
                    { 23, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Beton dökülmeden gider borularını sabitle.", false, true, "TipKat", "PlumbingType_Gomme", 1, "Tesisat Rezervasyon ve Borulama", null, 0, "Kaba" },
                    { 24, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Yapıştırıcıyı kıvamında hazırla.", false, false, "TipKat", "OuterWallType_Ytong", 3, "Dış Cephe Ytong Örümü", null, 0, "InceSabit" },
                    { 25, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Harç paylarına standartlara göre dikkat et.", false, false, "TipKat", "OuterWallType_Tugla", 3, "Dış Cephe Tuğla Örümü", null, 0, "InceSabit" },
                    { 26, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Motor kanca payını projeye göre bırak.", false, true, "Cati", "ElevatorType_Klasik", 1, "Asansör Makine Dairesi Betonu", null, 0, "Kaba" }
                });

            migrationBuilder.InsertData(
                table: "RecipeIngredients",
                columns: new[] { "Id", "CreatedAt", "IsDeleted", "MaterialName", "MaterialRecipeId", "Quantity", "Unit", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "20'lik Ytong Blok", 1, 6.5m, "Adet", null },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Ytong Yapıştırıcı", 1, 3.0m, "Kg", null },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "13.5'luk Tuğla", 2, 35.0m, "Adet", null },
                    { 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Çimento", 2, 15.0m, "Kg", null },
                    { 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Kum", 2, 40.0m, "Kg", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "MaterialRecipes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MaterialRecipes",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
