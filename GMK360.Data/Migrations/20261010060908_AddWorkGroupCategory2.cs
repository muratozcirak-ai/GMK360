using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkGroupCategory2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WorkGroupCategory",
                table: "WorkflowTemplates",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkGroupCategory",
                table: "ProjectTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 1,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 2,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 3,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 4,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 5,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 6,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 7,
                column: "WorkGroupCategory",
                value: "Kaba_Kalıp");

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 8,
                column: "WorkGroupCategory",
                value: "Kaba_Demir");

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 9,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 10,
                column: "WorkGroupCategory",
                value: "Kaba_Beton");

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 11,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 12,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 13,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 14,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 15,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 16,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 17,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 18,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 19,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 20,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 21,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 22,
                column: "WorkGroupCategory",
                value: null);

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 23,
                column: "WorkGroupCategory",
                value: "Tesisat_Mekanik");

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 24,
                column: "WorkGroupCategory",
                value: "Ince_Duvar");

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 25,
                column: "WorkGroupCategory",
                value: "Ince_Duvar");

            migrationBuilder.UpdateData(
                table: "WorkflowTemplates",
                keyColumn: "Id",
                keyValue: 26,
                column: "WorkGroupCategory",
                value: "Kaba_Kalıp_Demir");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkGroupCategory",
                table: "WorkflowTemplates");

            migrationBuilder.DropColumn(
                name: "WorkGroupCategory",
                table: "ProjectTasks");
        }
    }
}
