using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubcontractorHakedis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProgressPayments_ContractPhases_ContractPhaseId",
                table: "ProgressPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgressPayments_SubcontractorContracts_SubcontractorContractId",
                table: "ProgressPayments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProgressPayments",
                table: "ProgressPayments");

            migrationBuilder.RenameTable(
                name: "ProgressPayments",
                newName: "Hakedisler");

            migrationBuilder.RenameIndex(
                name: "IX_ProgressPayments_SubcontractorContractId",
                table: "Hakedisler",
                newName: "IX_Hakedisler_SubcontractorContractId");

            migrationBuilder.RenameIndex(
                name: "IX_ProgressPayments_ContractPhaseId",
                table: "Hakedisler",
                newName: "IX_Hakedisler_ContractPhaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Hakedisler",
                table: "Hakedisler",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "SubcontractorHakedisler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    HakedisNo = table.Column<int>(type: "int", nullable: false),
                    HakedisDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClaimAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeductionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeductionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubcontractorHakedisler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubcontractorHakedisler_SubcontractorContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "SubcontractorContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubcontractorHakedisler_ContractId",
                table: "SubcontractorHakedisler",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hakedisler_ContractPhases_ContractPhaseId",
                table: "Hakedisler",
                column: "ContractPhaseId",
                principalTable: "ContractPhases",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Hakedisler_SubcontractorContracts_SubcontractorContractId",
                table: "Hakedisler",
                column: "SubcontractorContractId",
                principalTable: "SubcontractorContracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hakedisler_ContractPhases_ContractPhaseId",
                table: "Hakedisler");

            migrationBuilder.DropForeignKey(
                name: "FK_Hakedisler_SubcontractorContracts_SubcontractorContractId",
                table: "Hakedisler");

            migrationBuilder.DropTable(
                name: "SubcontractorHakedisler");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Hakedisler",
                table: "Hakedisler");

            migrationBuilder.RenameTable(
                name: "Hakedisler",
                newName: "ProgressPayments");

            migrationBuilder.RenameIndex(
                name: "IX_Hakedisler_SubcontractorContractId",
                table: "ProgressPayments",
                newName: "IX_ProgressPayments_SubcontractorContractId");

            migrationBuilder.RenameIndex(
                name: "IX_Hakedisler_ContractPhaseId",
                table: "ProgressPayments",
                newName: "IX_ProgressPayments_ContractPhaseId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProgressPayments",
                table: "ProgressPayments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressPayments_ContractPhases_ContractPhaseId",
                table: "ProgressPayments",
                column: "ContractPhaseId",
                principalTable: "ContractPhases",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProgressPayments_SubcontractorContracts_SubcontractorContractId",
                table: "ProgressPayments",
                column: "SubcontractorContractId",
                principalTable: "SubcontractorContracts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
