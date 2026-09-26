using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentDependencies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Stage",
                table: "SystemLegalDocumentTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Stage",
                table: "ProjectLegalDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SystemDocumentDependency",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TargetDocumentId = table.Column<int>(type: "int", nullable: false),
                    PrerequisiteDocumentId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemDocumentDependency", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemDocumentDependency_SystemLegalDocumentTemplates_PrerequisiteDocumentId",
                        column: x => x.PrerequisiteDocumentId,
                        principalTable: "SystemLegalDocumentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemDocumentDependency_SystemLegalDocumentTemplates_TargetDocumentId",
                        column: x => x.TargetDocumentId,
                        principalTable: "SystemLegalDocumentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemDocumentDependency_PrerequisiteDocumentId",
                table: "SystemDocumentDependency",
                column: "PrerequisiteDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemDocumentDependency_TargetDocumentId",
                table: "SystemDocumentDependency",
                column: "TargetDocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemDocumentDependency");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "SystemLegalDocumentTemplates");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "ProjectLegalDocuments");
        }
    }
}
