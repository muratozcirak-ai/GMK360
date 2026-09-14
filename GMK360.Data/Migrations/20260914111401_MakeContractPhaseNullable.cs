using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeContractPhaseNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgressPayments_ContractPhases_ContractPhaseId",
                table: "ProgressPayments");

            migrationBuilder.AlterColumn<int>(
                name: "ContractPhaseId",
                table: "ProgressPayments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressPayments_ContractPhases_ContractPhaseId",
                table: "ProgressPayments",
                column: "ContractPhaseId",
                principalTable: "ContractPhases",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgressPayments_ContractPhases_ContractPhaseId",
                table: "ProgressPayments");

            migrationBuilder.AlterColumn<int>(
                name: "ContractPhaseId",
                table: "ProgressPayments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressPayments_ContractPhases_ContractPhaseId",
                table: "ProgressPayments",
                column: "ContractPhaseId",
                principalTable: "ContractPhases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
