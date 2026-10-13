using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkGroupToDna : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtraField1",
                table: "ProjectDnaDetails");

            migrationBuilder.DropColumn(
                name: "ExtraField2",
                table: "ProjectDnaDetails");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "ProjectDnaDetails");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "ProjectDnaDetails");

            migrationBuilder.RenameColumn(
                name: "B2B_SupplierId",
                table: "ProjectDnaDetails",
                newName: "WorkGroupDefinitionId");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ProjectDnaDetails",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "ProjectDnaDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "WorkGroupDefinition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    IsProcurementOnly = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkGroupDefinition", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDnaDetails_WorkGroupDefinitionId",
                table: "ProjectDnaDetails",
                column: "WorkGroupDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectDnaDetails_WorkGroupDefinition_WorkGroupDefinitionId",
                table: "ProjectDnaDetails",
                column: "WorkGroupDefinitionId",
                principalTable: "WorkGroupDefinition",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectDnaDetails_WorkGroupDefinition_WorkGroupDefinitionId",
                table: "ProjectDnaDetails");

            migrationBuilder.DropTable(
                name: "WorkGroupDefinition");

            migrationBuilder.DropIndex(
                name: "IX_ProjectDnaDetails_WorkGroupDefinitionId",
                table: "ProjectDnaDetails");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ProjectDnaDetails");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "ProjectDnaDetails");

            migrationBuilder.RenameColumn(
                name: "WorkGroupDefinitionId",
                table: "ProjectDnaDetails",
                newName: "B2B_SupplierId");

            migrationBuilder.AddColumn<string>(
                name: "ExtraField1",
                table: "ProjectDnaDetails",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtraField2",
                table: "ProjectDnaDetails",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPrice",
                table: "ProjectDnaDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "ProjectDnaDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
