using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowCoreEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequiredCondition",
                table: "WorkflowTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalQuantity",
                table: "ProjectTasks",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "WorkItemCode",
                table: "ProjectTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlockName",
                table: "ProjectElevations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLockedByIsg",
                table: "ProjectElevations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IsgLockReason",
                table: "ProjectElevations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentElevationId",
                table: "ProjectElevations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ElevatorType",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InnerWallType",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OuterWallType",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlumbingType",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoofType",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IsgAuditForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ElevationId = table.Column<int>(type: "int", nullable: true),
                    AuditDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AuditorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsSiteSafe = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IsgAuditForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IsgAuditForms_ConstructionProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "ConstructionProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IsgAuditForms_ProjectElevations_ElevationId",
                        column: x => x.ElevationId,
                        principalTable: "ProjectElevations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MaterialRecipes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkItemCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BaseUnit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialRecipes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IsgChecklistItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsgAuditFormId = table.Column<int>(type: "int", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPassed = table.Column<bool>(type: "bit", nullable: false),
                    IsCritical = table.Column<bool>(type: "bit", nullable: false),
                    ActionRequired = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IsgChecklistItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IsgChecklistItems_IsgAuditForms_IsgAuditFormId",
                        column: x => x.IsgAuditFormId,
                        principalTable: "IsgAuditForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecipeIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaterialRecipeId = table.Column<int>(type: "int", nullable: false),
                    MaterialName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecipeIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecipeIngredients_MaterialRecipes_MaterialRecipeId",
                        column: x => x.MaterialRecipeId,
                        principalTable: "MaterialRecipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 2,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 3,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 4,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 5,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 6,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 7,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 8,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 9,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 10,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 11,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 12,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 13,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 14,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 15,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 16,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 17,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 18,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 19,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 20,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 21,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 22,
                column: "RequiredCondition",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectElevations_ParentElevationId",
                table: "ProjectElevations",
                column: "ParentElevationId");

            migrationBuilder.CreateIndex(
                name: "IX_IsgAuditForms_ElevationId",
                table: "IsgAuditForms",
                column: "ElevationId");

            migrationBuilder.CreateIndex(
                name: "IX_IsgAuditForms_ProjectId",
                table: "IsgAuditForms",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_IsgChecklistItems_IsgAuditFormId",
                table: "IsgChecklistItems",
                column: "IsgAuditFormId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_MaterialRecipeId",
                table: "RecipeIngredients",
                column: "MaterialRecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectElevations_ProjectElevations_ParentElevationId",
                table: "ProjectElevations",
                column: "ParentElevationId",
                principalTable: "ProjectElevations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectElevations_ProjectElevations_ParentElevationId",
                table: "ProjectElevations");

            migrationBuilder.DropTable(
                name: "IsgChecklistItems");

            migrationBuilder.DropTable(
                name: "RecipeIngredients");

            migrationBuilder.DropTable(
                name: "IsgAuditForms");

            migrationBuilder.DropTable(
                name: "MaterialRecipes");

            migrationBuilder.DropIndex(
                name: "IX_ProjectElevations_ParentElevationId",
                table: "ProjectElevations");

            migrationBuilder.DropColumn(
                name: "RequiredCondition",
                table: "WorkflowTemplates");

            migrationBuilder.DropColumn(
                name: "TotalQuantity",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "WorkItemCode",
                table: "ProjectTasks");

            migrationBuilder.DropColumn(
                name: "BlockName",
                table: "ProjectElevations");

            migrationBuilder.DropColumn(
                name: "IsLockedByIsg",
                table: "ProjectElevations");

            migrationBuilder.DropColumn(
                name: "IsgLockReason",
                table: "ProjectElevations");

            migrationBuilder.DropColumn(
                name: "ParentElevationId",
                table: "ProjectElevations");

            migrationBuilder.DropColumn(
                name: "ElevatorType",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "InnerWallType",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "OuterWallType",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "PlumbingType",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "RoofType",
                table: "ConstructionProjects");
        }
    }
}
