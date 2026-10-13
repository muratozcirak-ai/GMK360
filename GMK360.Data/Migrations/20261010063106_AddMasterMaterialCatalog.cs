using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMasterMaterialCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaterialName",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "RecipeIngredients");

            migrationBuilder.AddColumn<int>(
                name: "MasterMaterialId",
                table: "RecipeIngredients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsMaterialReady",
                table: "ProjectTasks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MaterialShortageNote",
                table: "ProjectTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedStartDate",
                table: "ProjectTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MasterMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterialCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MaterialName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaseUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterMaterials", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "MasterMaterials",
                columns: new[] { "Id", "BaseUnit", "Category", "CreatedAt", "IsDeleted", "MaterialCode", "MaterialName", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Adet", "Ince_Duvar", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "MAT-YTONG-20", "20'lik Ytong Blok", null },
                    { 2, "Kg", "Ince_Duvar", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "MAT-YAPISTIRICI", "Örgü Tutkalı / Yapıştırıcı", null },
                    { 3, "Adet", "Ince_Duvar", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "MAT-TUGLA-13", "13.5'luk Harman Tuğla", null },
                    { 4, "Torba", "Ince_Duvar", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "MAT-CIMENTO-50", "50 Kg Torba Çimento", null },
                    { 5, "Ton", "Ince_Duvar", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "MAT-KUM", "Sıva Kumu", null }
                });

            migrationBuilder.UpdateData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: 1,
                column: "MasterMaterialId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: 2,
                column: "MasterMaterialId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: 3,
                column: "MasterMaterialId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: 4,
                column: "MasterMaterialId",
                value: 4);

            migrationBuilder.UpdateData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: 5,
                column: "MasterMaterialId",
                value: 5);

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_MasterMaterialId",
                table: "RecipeIngredients",
                column: "MasterMaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecipeIngredients_MasterMaterials_MasterMaterialId",
                table: "RecipeIngredients",
                column: "MasterMaterialId",
                principalTable: "MasterMaterials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecipeIngredients_MasterMaterials_MasterMaterialId",
                table: "RecipeIngredients");

            migrationBuilder.DropTable(
                name: "MasterMaterials");

            migrationBuilder.DropIndex(
                name: "IX_RecipeIngredients_MasterMaterialId",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "MasterMaterialId",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "IsMaterialReady",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "MaterialShortageNote",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "PlannedStartDate",
                table: "ProjectTasks");

            migrationBuilder.AddColumn<string>(
                name: "MaterialName",
                table: "RecipeIngredients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "RecipeIngredients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "MaterialName", "Unit" },
                values: new object[] { "20'lik Ytong Blok", "Adet" });

            migrationBuilder.UpdateData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "MaterialName", "Unit" },
                values: new object[] { "Ytong Yapıştırıcı", "Kg" });

            migrationBuilder.UpdateData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "MaterialName", "Unit" },
                values: new object[] { "13.5'luk Tuğla", "Adet" });

            migrationBuilder.UpdateData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "MaterialName", "Unit" },
                values: new object[] { "Çimento", "Kg" });

            migrationBuilder.UpdateData(
                table: "RecipeIngredients",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "MaterialName", "Unit" },
                values: new object[] { "Kum", "Kg" });
        }
    }
}
