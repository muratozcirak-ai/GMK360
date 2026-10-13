using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectScheduleTasks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DxfUnitTypeProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    ModelName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalGrossArea = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalNetArea = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalSpaceCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DxfUnitTypeProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectDnaProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExtraField1 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraField2 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDnaProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectDnaProfiles_ConstructionProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "ConstructionProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectScheduleTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    TaskName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FloorOrZone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlannedDurationDays = table.Column<int>(type: "int", nullable: false),
                    PlannedStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectScheduleTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DxfUnitSpaces",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DxfUnitTypeProfileId = table.Column<int>(type: "int", nullable: false),
                    SpaceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SpaceGroup = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FloorArea = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CeilingArea = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GrossWallArea = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetWallArea = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SkirtingLength = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WindowSillLength = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InteriorDoorCount = table.Column<int>(type: "int", nullable: false),
                    WindowCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DxfUnitSpaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DxfUnitSpaces_DxfUnitTypeProfiles_DxfUnitTypeProfileId",
                        column: x => x.DxfUnitTypeProfileId,
                        principalTable: "DxfUnitTypeProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectDnaDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProfileId = table.Column<int>(type: "int", nullable: false),
                    Phase = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SelectedItem = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    B2B_SupplierId = table.Column<int>(type: "int", nullable: true),
                    ExtraField1 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExtraField2 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDnaDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectDnaDetails_ProjectDnaProfiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "ProjectDnaProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectScheduleSubTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectScheduleTaskId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    RequiresPhoto = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectScheduleSubTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectScheduleSubTasks_ProjectScheduleTasks_ProjectScheduleTaskId",
                        column: x => x.ProjectScheduleTaskId,
                        principalTable: "ProjectScheduleTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DxfUnitSpaces_DxfUnitTypeProfileId",
                table: "DxfUnitSpaces",
                column: "DxfUnitTypeProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDnaDetails_ProfileId",
                table: "ProjectDnaDetails",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDnaProfiles_ProjectId",
                table: "ProjectDnaProfiles",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectScheduleSubTasks_ProjectScheduleTaskId",
                table: "ProjectScheduleSubTasks",
                column: "ProjectScheduleTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DxfUnitSpaces");

            migrationBuilder.DropTable(
                name: "ProjectDnaDetails");

            migrationBuilder.DropTable(
                name: "ProjectScheduleSubTasks");

            migrationBuilder.DropTable(
                name: "DxfUnitTypeProfiles");

            migrationBuilder.DropTable(
                name: "ProjectDnaProfiles");

            migrationBuilder.DropTable(
                name: "ProjectScheduleTasks");
        }
    }
}
