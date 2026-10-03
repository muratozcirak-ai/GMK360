using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFeasibilitySelected : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConstructionProjectExpenses_ConstructionProjects_ProjectId",
                table: "ConstructionProjectExpenses");

            migrationBuilder.AddColumn<int>(
                name: "EndKm",
                table: "VehicleTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EndTime",
                table: "VehicleTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartKm",
                table: "VehicleTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartTime",
                table: "VehicleTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WorkingHours",
                table: "VehicleTasks",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OdometerAtExpense",
                table: "VehicleExpenses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "VehicleExpenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalLocation",
                table: "ProjectLegalDocuments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "ConstructionProjectExpenses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "ConstructionProjectExpenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentKm",
                table: "CompanyVehicles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentWorkingHours",
                table: "CompanyVehicles",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeasibilitySelected",
                table: "B2BQuoteInvites",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_ConstructionProjectExpenses_ConstructionProjects_ProjectId",
                table: "ConstructionProjectExpenses",
                column: "ProjectId",
                principalTable: "ConstructionProjects",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConstructionProjectExpenses_ConstructionProjects_ProjectId",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "EndKm",
                table: "VehicleTasks");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "VehicleTasks");

            migrationBuilder.DropColumn(
                name: "StartKm",
                table: "VehicleTasks");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "VehicleTasks");

            migrationBuilder.DropColumn(
                name: "WorkingHours",
                table: "VehicleTasks");

            migrationBuilder.DropColumn(
                name: "OdometerAtExpense",
                table: "VehicleExpenses");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "VehicleExpenses");

            migrationBuilder.DropColumn(
                name: "OriginalLocation",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "CurrentKm",
                table: "CompanyVehicles");

            migrationBuilder.DropColumn(
                name: "CurrentWorkingHours",
                table: "CompanyVehicles");

            migrationBuilder.DropColumn(
                name: "IsFeasibilitySelected",
                table: "B2BQuoteInvites");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "ConstructionProjectExpenses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ConstructionProjectExpenses_ConstructionProjects_ProjectId",
                table: "ConstructionProjectExpenses",
                column: "ProjectId",
                principalTable: "ConstructionProjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
