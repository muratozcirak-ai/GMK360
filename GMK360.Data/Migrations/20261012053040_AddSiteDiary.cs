using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteDiary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectScheduleTaskId",
                table: "ProjectMaterialDemands",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProjectScheduleTaskId",
                table: "DailyTimesheets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SiteDiaryTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    BagliOlduguAnaGorevId = table.Column<int>(type: "int", nullable: true),
                    GorevTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GorevTanimi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GorevTipi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZimmetliPersonelId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KanitFotografUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OnaylayanKullaniciId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SiteDiaryTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SiteDiaryTasks_ConstructionProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "ConstructionProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaketAdi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AktifMi = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaskActionTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskTemplateId = table.Column<int>(type: "int", nullable: false),
                    GorevTanimi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZamanOfsetiGun = table.Column<int>(type: "int", nullable: false),
                    HedefRol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskActionTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskActionTemplates_TaskTemplates_TaskTemplateId",
                        column: x => x.TaskTemplateId,
                        principalTable: "TaskTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskChecklistTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskTemplateId = table.Column<int>(type: "int", nullable: false),
                    KontrolMaddesi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FotografZorunluMu = table.Column<bool>(type: "bit", nullable: false),
                    IseEngelMi = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskChecklistTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskChecklistTemplates_TaskTemplates_TaskTemplateId",
                        column: x => x.TaskTemplateId,
                        principalTable: "TaskTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectMaterialDemands_ProjectScheduleTaskId",
                table: "ProjectMaterialDemands",
                column: "ProjectScheduleTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyTimesheets_ProjectScheduleTaskId",
                table: "DailyTimesheets",
                column: "ProjectScheduleTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_SiteDiaryTasks_ProjectId",
                table: "SiteDiaryTasks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskActionTemplates_TaskTemplateId",
                table: "TaskActionTemplates",
                column: "TaskTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskChecklistTemplates_TaskTemplateId",
                table: "TaskChecklistTemplates",
                column: "TaskTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyTimesheets_ProjectScheduleTasks_ProjectScheduleTaskId",
                table: "DailyTimesheets",
                column: "ProjectScheduleTaskId",
                principalTable: "ProjectScheduleTasks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectMaterialDemands_ProjectScheduleTasks_ProjectScheduleTaskId",
                table: "ProjectMaterialDemands",
                column: "ProjectScheduleTaskId",
                principalTable: "ProjectScheduleTasks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyTimesheets_ProjectScheduleTasks_ProjectScheduleTaskId",
                table: "DailyTimesheets");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectMaterialDemands_ProjectScheduleTasks_ProjectScheduleTaskId",
                table: "ProjectMaterialDemands");

            migrationBuilder.DropTable(
                name: "SiteDiaryTasks");

            migrationBuilder.DropTable(
                name: "TaskActionTemplates");

            migrationBuilder.DropTable(
                name: "TaskChecklistTemplates");

            migrationBuilder.DropTable(
                name: "TaskTemplates");

            migrationBuilder.DropIndex(
                name: "IX_ProjectMaterialDemands_ProjectScheduleTaskId",
                table: "ProjectMaterialDemands");

            migrationBuilder.DropIndex(
                name: "IX_DailyTimesheets_ProjectScheduleTaskId",
                table: "DailyTimesheets");

            migrationBuilder.DropColumn(
                name: "ProjectScheduleTaskId",
                table: "ProjectMaterialDemands");

            migrationBuilder.DropColumn(
                name: "ProjectScheduleTaskId",
                table: "DailyTimesheets");
        }
    }
}
