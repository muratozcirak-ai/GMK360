using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndAmenities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "LandscapeArea",
                table: "ConstructionProjects",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "TotalLandArea",
                table: "ConstructionProjects",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "BaseArea",
                table: "Buildings",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FacadeDirection",
                table: "Buildings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProjectAmenities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConstructionProjectId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SquareMeters = table.Column<double>(type: "float", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectAmenities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectAmenities_ConstructionProjects_ConstructionProjectId",
                        column: x => x.ConstructionProjectId,
                        principalTable: "ConstructionProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConstructionProjectId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleInProject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectAssignments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectAssignments_ConstructionProjects_ConstructionProjectId",
                        column: x => x.ConstructionProjectId,
                        principalTable: "ConstructionProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAmenities_ConstructionProjectId",
                table: "ProjectAmenities",
                column: "ConstructionProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssignments_ConstructionProjectId",
                table: "ProjectAssignments",
                column: "ConstructionProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectAssignments_UserId",
                table: "ProjectAssignments",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectAmenities");

            migrationBuilder.DropTable(
                name: "ProjectAssignments");

            migrationBuilder.DropColumn(
                name: "LandscapeArea",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "TotalLandArea",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "BaseArea",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "FacadeDirection",
                table: "Buildings");
        }
    }
}
