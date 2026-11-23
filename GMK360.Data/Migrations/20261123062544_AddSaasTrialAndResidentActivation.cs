using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSaasTrialAndResidentActivation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "tbl_gmk_KullaniciAbonelikleri",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountPolicyId",
                table: "tbl_gmk_KullaniciAbonelikleri",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPaymentDate",
                table: "tbl_gmk_KullaniciAbonelikleri",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "tbl_gmk_KullaniciAbonelikleri",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "DiscountPolicies",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "ExtraTrialDays",
                table: "DiscountPolicies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PromoCode",
                table: "DiscountPolicies",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetModule",
                table: "DiscountPolicies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActivationToken",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivationTokenExpiresAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PhoneVerifiedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tbl_gmk_KullaniciAbonelikleri_DiscountPolicyId",
                table: "tbl_gmk_KullaniciAbonelikleri",
                column: "DiscountPolicyId");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_gmk_KullaniciAbonelikleri_DiscountPolicies_DiscountPolicyId",
                table: "tbl_gmk_KullaniciAbonelikleri",
                column: "DiscountPolicyId",
                principalTable: "DiscountPolicies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_gmk_KullaniciAbonelikleri_DiscountPolicies_DiscountPolicyId",
                table: "tbl_gmk_KullaniciAbonelikleri");

            migrationBuilder.DropIndex(
                name: "IX_tbl_gmk_KullaniciAbonelikleri_DiscountPolicyId",
                table: "tbl_gmk_KullaniciAbonelikleri");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "tbl_gmk_KullaniciAbonelikleri");

            migrationBuilder.DropColumn(
                name: "DiscountPolicyId",
                table: "tbl_gmk_KullaniciAbonelikleri");

            migrationBuilder.DropColumn(
                name: "LastPaymentDate",
                table: "tbl_gmk_KullaniciAbonelikleri");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "tbl_gmk_KullaniciAbonelikleri");

            migrationBuilder.DropColumn(
                name: "ExtraTrialDays",
                table: "DiscountPolicies");

            migrationBuilder.DropColumn(
                name: "PromoCode",
                table: "DiscountPolicies");

            migrationBuilder.DropColumn(
                name: "TargetModule",
                table: "DiscountPolicies");

            migrationBuilder.DropColumn(
                name: "ActivationToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ActivationTokenExpiresAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PhoneVerifiedAt",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "DiscountPolicies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
