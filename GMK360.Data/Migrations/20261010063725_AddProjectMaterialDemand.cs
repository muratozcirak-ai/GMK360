using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectMaterialDemand : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectMaterialDemands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ElevationId = table.Column<int>(type: "int", nullable: true),
                    MasterMaterialId = table.Column<int>(type: "int", nullable: false),
                    TotalRequiredQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsOrdered = table.Column<bool>(type: "bit", nullable: false),
                    IsDeliveredToSite = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectMaterialDemands", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectMaterialDemands_MasterMaterials_MasterMaterialId",
                        column: x => x.MasterMaterialId,
                        principalTable: "MasterMaterials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMaterialDemands_MasterMaterialId",
                table: "ProjectMaterialDemands",
                column: "MasterMaterialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectMaterialDemands");
        }
    }
}
