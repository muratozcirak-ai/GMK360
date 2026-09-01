using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase4_AgreementsAndSms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresInvoice",
                table: "WalletWithdrawalRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UploadedInvoiceDocumentId",
                table: "WalletWithdrawalRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LegalAgreements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalAgreements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ManagementDecisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingId = table.Column<int>(type: "int", nullable: true),
                    HousingComplexId = table.Column<int>(type: "int", nullable: true),
                    DecisionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DocumentImageId = table.Column<int>(type: "int", nullable: true),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ManagementDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ManagementDecisions_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ManagementDecisions_HousingComplexes_HousingComplexId",
                        column: x => x.HousingComplexId,
                        principalTable: "HousingComplexes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SmsTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserAgendas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AgendaType = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAgendas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAgendas_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAgreementAcceptances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LegalAgreementId = table.Column<int>(type: "int", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAgreementAcceptances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAgreementAcceptances_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAgreementAcceptances_LegalAgreements_LegalAgreementId",
                        column: x => x.LegalAgreementId,
                        principalTable: "LegalAgreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "LegalAgreements",
                columns: new[] { "Id", "Content", "CreatedAt", "IsActive", "IsDeleted", "IsRequired", "Title", "Type", "UpdatedAt", "Version" },
                values: new object[,]
                {
                    { 1, "<h1>Kullanıcı Sözleşmesi</h1><p>Okudum, anladım...</p>", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, true, "Kullanıcı Sözleşmesi", 1, null, "1.0" },
                    { 2, "<h1>KVKK Metni</h1><p>Kişisel verileriniz...</p>", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, true, "KVKK Aydınlatma Metni", 2, null, "1.0" },
                    { 3, "<h1>SMS Onayı</h1><p>Tarafıma bilgilendirme SMS'leri atılmasına onay veriyorum.</p>", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, true, "SMS Bilgilendirme Onayı", 3, null, "1.0" },
                    { 4, "<h1>Referans Sistemi Onayı</h1><p>Kazanılan hak edişlerden yasal oranlarda stopaj kesilecektir.</p>", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, true, "Referans ve Gelir Sözleşmesi", 4, null, "1.0" }
                });

            migrationBuilder.InsertData(
                table: "SmsTemplates",
                columns: new[] { "Id", "Body", "CreatedAt", "IsActive", "IsDeleted", "TemplateCode", "Title", "UpdatedAt" },
                values: new object[] { 1, "Sayın {FullName}, GMK360 platformuna hoşgeldiniz.", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "USER_REGISTER", "Kayıt SMS'i", null });

            migrationBuilder.CreateIndex(
                name: "IX_ManagementDecisions_BuildingId",
                table: "ManagementDecisions",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_ManagementDecisions_HousingComplexId",
                table: "ManagementDecisions",
                column: "HousingComplexId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAgendas_UserId",
                table: "UserAgendas",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAgreementAcceptances_LegalAgreementId",
                table: "UserAgreementAcceptances",
                column: "LegalAgreementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAgreementAcceptances_UserId",
                table: "UserAgreementAcceptances",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ManagementDecisions");

            migrationBuilder.DropTable(
                name: "SmsTemplates");

            migrationBuilder.DropTable(
                name: "UserAgendas");

            migrationBuilder.DropTable(
                name: "UserAgreementAcceptances");

            migrationBuilder.DropTable(
                name: "LegalAgreements");

            migrationBuilder.DropColumn(
                name: "RequiresInvoice",
                table: "WalletWithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "UploadedInvoiceDocumentId",
                table: "WalletWithdrawalRequests");
        }
    }
}
