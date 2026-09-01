using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddManagementCompanyHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildingExpenses_Buildings_BuildingId",
                table: "BuildingExpenses");

            migrationBuilder.AddColumn<int>(
                name: "BuildingId",
                table: "RenovationRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HousingComplexId",
                table: "RenovationRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCommonArea",
                table: "RenovationRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ScopeType",
                table: "RenovationRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "TenantPhone",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TenantName",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TenantEmail",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerPhone",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerName",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerEmail",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "BuildingUnits",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TenantUserId",
                table: "BuildingUnits",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ManagerUserId",
                table: "Buildings",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "HousingComplexId",
                table: "Buildings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "Buildings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalUnits",
                table: "Buildings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "BuildingId",
                table: "BuildingExpenses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "HousingComplexId",
                table: "BuildingExpenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScopeType",
                table: "BuildingExpenses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsShadowAccount",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DocumentArchives",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentArchives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentArchives_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ManagementCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subdomain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagementCompanies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Meetings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MeetingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Agenda = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinutesUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Meetings_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HousingComplexes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ManagementCompanyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalBlocks = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HousingComplexes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HousingComplexes_ManagementCompanies_ManagementCompanyId",
                        column: x => x.ManagementCompanyId,
                        principalTable: "ManagementCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RenovationRequests_BuildingId",
                table: "RenovationRequests",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_RenovationRequests_HousingComplexId",
                table: "RenovationRequests",
                column: "HousingComplexId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_OwnerUserId",
                table: "BuildingUnits",
                column: "OwnerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingUnits_TenantUserId",
                table: "BuildingUnits",
                column: "TenantUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_HousingComplexId",
                table: "Buildings",
                column: "HousingComplexId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_ManagerUserId",
                table: "Buildings",
                column: "ManagerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingExpenses_HousingComplexId",
                table: "BuildingExpenses",
                column: "HousingComplexId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentArchives_BuildingId",
                table: "DocumentArchives",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingComplexes_ManagementCompanyId",
                table: "HousingComplexes",
                column: "ManagementCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_BuildingId",
                table: "Meetings",
                column: "BuildingId");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingExpenses_Buildings_BuildingId",
                table: "BuildingExpenses",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingExpenses_HousingComplexes_HousingComplexId",
                table: "BuildingExpenses",
                column: "HousingComplexId",
                principalTable: "HousingComplexes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_AspNetUsers_ManagerUserId",
                table: "Buildings",
                column: "ManagerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_HousingComplexes_HousingComplexId",
                table: "Buildings",
                column: "HousingComplexId",
                principalTable: "HousingComplexes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingUnits_AspNetUsers_OwnerUserId",
                table: "BuildingUnits",
                column: "OwnerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingUnits_AspNetUsers_TenantUserId",
                table: "BuildingUnits",
                column: "TenantUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RenovationRequests_Buildings_BuildingId",
                table: "RenovationRequests",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RenovationRequests_HousingComplexes_HousingComplexId",
                table: "RenovationRequests",
                column: "HousingComplexId",
                principalTable: "HousingComplexes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildingExpenses_Buildings_BuildingId",
                table: "BuildingExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_BuildingExpenses_HousingComplexes_HousingComplexId",
                table: "BuildingExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_AspNetUsers_ManagerUserId",
                table: "Buildings");

            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_HousingComplexes_HousingComplexId",
                table: "Buildings");

            migrationBuilder.DropForeignKey(
                name: "FK_BuildingUnits_AspNetUsers_OwnerUserId",
                table: "BuildingUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_BuildingUnits_AspNetUsers_TenantUserId",
                table: "BuildingUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_RenovationRequests_Buildings_BuildingId",
                table: "RenovationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RenovationRequests_HousingComplexes_HousingComplexId",
                table: "RenovationRequests");

            migrationBuilder.DropTable(
                name: "DocumentArchives");

            migrationBuilder.DropTable(
                name: "HousingComplexes");

            migrationBuilder.DropTable(
                name: "Meetings");

            migrationBuilder.DropTable(
                name: "ManagementCompanies");

            migrationBuilder.DropIndex(
                name: "IX_RenovationRequests_BuildingId",
                table: "RenovationRequests");

            migrationBuilder.DropIndex(
                name: "IX_RenovationRequests_HousingComplexId",
                table: "RenovationRequests");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUnits_OwnerUserId",
                table: "BuildingUnits");

            migrationBuilder.DropIndex(
                name: "IX_BuildingUnits_TenantUserId",
                table: "BuildingUnits");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_HousingComplexId",
                table: "Buildings");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_ManagerUserId",
                table: "Buildings");

            migrationBuilder.DropIndex(
                name: "IX_BuildingExpenses_HousingComplexId",
                table: "BuildingExpenses");

            migrationBuilder.DropColumn(
                name: "BuildingId",
                table: "RenovationRequests");

            migrationBuilder.DropColumn(
                name: "HousingComplexId",
                table: "RenovationRequests");

            migrationBuilder.DropColumn(
                name: "IsCommonArea",
                table: "RenovationRequests");

            migrationBuilder.DropColumn(
                name: "ScopeType",
                table: "RenovationRequests");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "TenantUserId",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "HousingComplexId",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "TotalUnits",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "HousingComplexId",
                table: "BuildingExpenses");

            migrationBuilder.DropColumn(
                name: "ScopeType",
                table: "BuildingExpenses");

            migrationBuilder.DropColumn(
                name: "IsShadowAccount",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "TenantPhone",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantName",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantEmail",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerPhone",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerName",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerEmail",
                table: "BuildingUnits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ManagerUserId",
                table: "Buildings",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "BuildingId",
                table: "BuildingExpenses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingExpenses_Buildings_BuildingId",
                table: "BuildingExpenses",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
