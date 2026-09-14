using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFinanceModuleToPhonebook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubcontractorContracts_B2BNetworkContacts_SubcontractorId",
                table: "SubcontractorContracts");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierCurrentAccounts_B2BNetworkContacts_NetworkContactId",
                table: "SupplierCurrentAccounts");

            migrationBuilder.RenameColumn(
                name: "NetworkContactId",
                table: "SupplierCurrentAccounts",
                newName: "PhonebookContactId");

            migrationBuilder.RenameIndex(
                name: "IX_SupplierCurrentAccounts_NetworkContactId",
                table: "SupplierCurrentAccounts",
                newName: "IX_SupplierCurrentAccounts_PhonebookContactId");

            migrationBuilder.RenameColumn(
                name: "SubcontractorId",
                table: "SubcontractorContracts",
                newName: "PhonebookContactId");

            migrationBuilder.RenameIndex(
                name: "IX_SubcontractorContracts_SubcontractorId",
                table: "SubcontractorContracts",
                newName: "IX_SubcontractorContracts_PhonebookContactId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubcontractorContracts_AgencyPhonebooks_PhonebookContactId",
                table: "SubcontractorContracts",
                column: "PhonebookContactId",
                principalTable: "AgencyPhonebooks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierCurrentAccounts_AgencyPhonebooks_PhonebookContactId",
                table: "SupplierCurrentAccounts",
                column: "PhonebookContactId",
                principalTable: "AgencyPhonebooks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubcontractorContracts_AgencyPhonebooks_PhonebookContactId",
                table: "SubcontractorContracts");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierCurrentAccounts_AgencyPhonebooks_PhonebookContactId",
                table: "SupplierCurrentAccounts");

            migrationBuilder.RenameColumn(
                name: "PhonebookContactId",
                table: "SupplierCurrentAccounts",
                newName: "NetworkContactId");

            migrationBuilder.RenameIndex(
                name: "IX_SupplierCurrentAccounts_PhonebookContactId",
                table: "SupplierCurrentAccounts",
                newName: "IX_SupplierCurrentAccounts_NetworkContactId");

            migrationBuilder.RenameColumn(
                name: "PhonebookContactId",
                table: "SubcontractorContracts",
                newName: "SubcontractorId");

            migrationBuilder.RenameIndex(
                name: "IX_SubcontractorContracts_PhonebookContactId",
                table: "SubcontractorContracts",
                newName: "IX_SubcontractorContracts_SubcontractorId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubcontractorContracts_B2BNetworkContacts_SubcontractorId",
                table: "SubcontractorContracts",
                column: "SubcontractorId",
                principalTable: "B2BNetworkContacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierCurrentAccounts_B2BNetworkContacts_NetworkContactId",
                table: "SupplierCurrentAccounts",
                column: "NetworkContactId",
                principalTable: "B2BNetworkContacts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
