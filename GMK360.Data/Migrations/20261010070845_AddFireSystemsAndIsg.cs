using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFireSystemsAndIsg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FireAlarmSystem",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FireExtinguishingSystem",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MasterIsgQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhaseCategory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCritical = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterIsgQuestions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "MasterIsgQuestions",
                columns: new[] { "Id", "CreatedAt", "IsCritical", "IsDeleted", "PhaseCategory", "QuestionText", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, false, "Temel", "Derin kazı destekleri (iksa) güvenli mi?", null },
                    { 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, false, "TipKat", "Şaft boşluklarında düşmeyi önleyici korkuluk var mı?", null },
                    { 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, false, "Cati", "Yaşam hatları kurulu mu?", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MasterIsgQuestions");

            migrationBuilder.DropColumn(
                name: "FireAlarmSystem",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "FireExtinguishingSystem",
                table: "ConstructionProjects");
        }
    }
}
