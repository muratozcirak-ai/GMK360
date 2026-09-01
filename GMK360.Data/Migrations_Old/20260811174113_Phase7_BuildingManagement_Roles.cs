using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase7_BuildingManagement_Roles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ShadowCreatorId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_BuildingExpenses_ExpenseCategories_ExpenseCategoryId",
                table: "BuildingExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_GlobalObligationRules_ExpenseCategories_TargetExpenseCategoryId",
                table: "GlobalObligationRules");

            migrationBuilder.DropForeignKey(
                name: "FK_RenovationRequestInvites_AspNetUsers_ServiceProviderId",
                table: "RenovationRequestInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_RenovationRequestInvites_RenovationRequests_RenovationRequestId",
                table: "RenovationRequestInvites");

            migrationBuilder.DropTable(
                name: "ContractAcceptanceLogs");

            migrationBuilder.DropTable(
                name: "PropertyUsers");

            migrationBuilder.DropTable(
                name: "SystemDocuments");

            migrationBuilder.DropTable(
                name: "VotingLogs");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_CorporateProfiles_ApplicationUserId",
                table: "CorporateProfiles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ShadowCreatorId",
                table: "AspNetUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RenovationRequestInvites",
                table: "RenovationRequestInvites");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExpenseCategories",
                table: "ExpenseCategories");

            migrationBuilder.DeleteData(
                table: "Institutions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Institutions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Institutions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Institutions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Institutions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "LegalAgreements",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "LegalAgreements",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "LegalAgreements",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "LegalAgreements",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "SmsTemplates",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.RenameTable(
                name: "RenovationRequestInvites",
                newName: "RenovationRequestInvite");

            migrationBuilder.RenameTable(
                name: "ExpenseCategories",
                newName: "ExpenseCategory");

            migrationBuilder.RenameIndex(
                name: "IX_RenovationRequestInvites_ServiceProviderId",
                table: "RenovationRequestInvite",
                newName: "IX_RenovationRequestInvite_ServiceProviderId");

            migrationBuilder.RenameIndex(
                name: "IX_RenovationRequestInvites_RenovationRequestId",
                table: "RenovationRequestInvite",
                newName: "IX_RenovationRequestInvite_RenovationRequestId");

            migrationBuilder.AddColumn<int>(
                name: "OnboardingStep",
                table: "Buildings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ShadowCreatorId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RenovationRequestInvite",
                table: "RenovationRequestInvite",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExpenseCategory",
                table: "ExpenseCategory",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BuildingManagers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingManagers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildingManagers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuildingManagers_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BuildingMeetings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingId = table.Column<int>(type: "int", nullable: false),
                    MeetingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AgendaTitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    IsConcluded = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingMeetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuildingMeetings_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeetingDecisions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingMeetingId = table.Column<int>(type: "int", nullable: false),
                    DecisionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingDecisions_BuildingMeetings_BuildingMeetingId",
                        column: x => x.BuildingMeetingId,
                        principalTable: "BuildingMeetings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorporateProfiles_ApplicationUserId",
                table: "CorporateProfiles",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingManagers_BuildingId",
                table: "BuildingManagers",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingManagers_UserId",
                table: "BuildingManagers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BuildingMeetings_BuildingId",
                table: "BuildingMeetings",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingDecisions_BuildingMeetingId",
                table: "MeetingDecisions",
                column: "BuildingMeetingId");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingExpenses_ExpenseCategory_ExpenseCategoryId",
                table: "BuildingExpenses",
                column: "ExpenseCategoryId",
                principalTable: "ExpenseCategory",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GlobalObligationRules_ExpenseCategory_TargetExpenseCategoryId",
                table: "GlobalObligationRules",
                column: "TargetExpenseCategoryId",
                principalTable: "ExpenseCategory",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RenovationRequestInvite_AspNetUsers_ServiceProviderId",
                table: "RenovationRequestInvite",
                column: "ServiceProviderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RenovationRequestInvite_RenovationRequests_RenovationRequestId",
                table: "RenovationRequestInvite",
                column: "RenovationRequestId",
                principalTable: "RenovationRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BuildingExpenses_ExpenseCategory_ExpenseCategoryId",
                table: "BuildingExpenses");

            migrationBuilder.DropForeignKey(
                name: "FK_GlobalObligationRules_ExpenseCategory_TargetExpenseCategoryId",
                table: "GlobalObligationRules");

            migrationBuilder.DropForeignKey(
                name: "FK_RenovationRequestInvite_AspNetUsers_ServiceProviderId",
                table: "RenovationRequestInvite");

            migrationBuilder.DropForeignKey(
                name: "FK_RenovationRequestInvite_RenovationRequests_RenovationRequestId",
                table: "RenovationRequestInvite");

            migrationBuilder.DropTable(
                name: "BuildingManagers");

            migrationBuilder.DropTable(
                name: "MeetingDecisions");

            migrationBuilder.DropTable(
                name: "BuildingMeetings");

            migrationBuilder.DropIndex(
                name: "IX_CorporateProfiles_ApplicationUserId",
                table: "CorporateProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RenovationRequestInvite",
                table: "RenovationRequestInvite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExpenseCategory",
                table: "ExpenseCategory");

            migrationBuilder.DropColumn(
                name: "OnboardingStep",
                table: "Buildings");

            migrationBuilder.RenameTable(
                name: "RenovationRequestInvite",
                newName: "RenovationRequestInvites");

            migrationBuilder.RenameTable(
                name: "ExpenseCategory",
                newName: "ExpenseCategories");

            migrationBuilder.RenameIndex(
                name: "IX_RenovationRequestInvite_ServiceProviderId",
                table: "RenovationRequestInvites",
                newName: "IX_RenovationRequestInvites_ServiceProviderId");

            migrationBuilder.RenameIndex(
                name: "IX_RenovationRequestInvite_RenovationRequestId",
                table: "RenovationRequestInvites",
                newName: "IX_RenovationRequestInvites_RenovationRequestId");

            migrationBuilder.AlterColumn<string>(
                name: "ShadowCreatorId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RenovationRequestInvites",
                table: "RenovationRequestInvites",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExpenseCategories",
                table: "ExpenseCategories",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContentHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContractType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropertyUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RoleType = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyUsers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropertyUsers_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SystemDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UploadedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModuleType = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemDocuments_AspNetUsers_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VotingLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuditHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BuildingManagementId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ResidentPhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ResidentUnitNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentOtpCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotingLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractAcceptanceLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedVersion = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractAcceptanceLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractAcceptanceLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractAcceptanceLogs_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Institutions",
                columns: new[] { "Id", "CreatedAt", "InstitutionType", "IsApiSupported", "IsDeleted", "LogoUrl", "Name", "NormalizedCode", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, false, null, "TEDAŞ", "tedas", null },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, true, false, null, "İGDAŞ", "igdas", null },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, false, false, null, "İSKİ", "iski", null },
                    { 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 2, false, false, null, "ASAT", "asat", null },
                    { 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, true, false, null, "ENERJİSA", "enerjisa", null }
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
                name: "IX_CorporateProfiles_ApplicationUserId",
                table: "CorporateProfiles",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ShadowCreatorId",
                table: "AspNetUsers",
                column: "ShadowCreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAcceptanceLogs_ContractId",
                table: "ContractAcceptanceLogs",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAcceptanceLogs_UserId",
                table: "ContractAcceptanceLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyUsers_PropertyId",
                table: "PropertyUsers",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyUsers_UserId",
                table: "PropertyUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemDocuments_UploadedById",
                table: "SystemDocuments",
                column: "UploadedById");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ShadowCreatorId",
                table: "AspNetUsers",
                column: "ShadowCreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BuildingExpenses_ExpenseCategories_ExpenseCategoryId",
                table: "BuildingExpenses",
                column: "ExpenseCategoryId",
                principalTable: "ExpenseCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GlobalObligationRules_ExpenseCategories_TargetExpenseCategoryId",
                table: "GlobalObligationRules",
                column: "TargetExpenseCategoryId",
                principalTable: "ExpenseCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RenovationRequestInvites_AspNetUsers_ServiceProviderId",
                table: "RenovationRequestInvites",
                column: "ServiceProviderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RenovationRequestInvites_RenovationRequests_RenovationRequestId",
                table: "RenovationRequestInvites",
                column: "RenovationRequestId",
                principalTable: "RenovationRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
