using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConstructionBudgetItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemDocumentDependency");

            migrationBuilder.DropColumn(
                name: "IsMandatory",
                table: "SystemLegalDocumentTemplates");

            migrationBuilder.DropColumn(
                name: "TargetModule",
                table: "SystemLegalDocumentTemplates");

            migrationBuilder.RenameColumn(
                name: "Stage",
                table: "SystemLegalDocumentTemplates",
                newName: "Category");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ProjectLegalDocuments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "AdditionalCost",
                table: "ProjectLegalDocuments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "ProjectLegalDocuments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DocumentFee",
                table: "ProjectLegalDocuments",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetContactId",
                table: "ProjectLegalDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetInstitutionId",
                table: "ProjectLegalDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AgencyPhonebooks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorizedPerson",
                table: "AgencyPhonebooks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorizedPersonRole",
                table: "AgencyPhonebooks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AgencyPhonebooks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "AgencyPhonebooks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtensionNumber",
                table: "AgencyPhonebooks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleMapsUrl",
                table: "AgencyPhonebooks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Iban",
                table: "AgencyPhonebooks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LandlinePhone",
                table: "AgencyPhonebooks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "AgencyPhonebooks",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "AgencyPhonebooks",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MobilePhone2",
                table: "AgencyPhonebooks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "AgencyPhonebooks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxOffice",
                table: "AgencyPhonebooks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WebsiteUrl",
                table: "AgencyPhonebooks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AgencyPhonebookBranch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyPhonebookId = table.Column<int>(type: "int", nullable: false),
                    BranchName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    GoogleMapsUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyPhonebookBranch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgencyPhonebookBranch_AgencyPhonebooks_AgencyPhonebookId",
                        column: x => x.AgencyPhonebookId,
                        principalTable: "AgencyPhonebooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AgencyPhonebookContact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyPhonebookId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartmentOrRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExtensionNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyPhonebookContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgencyPhonebookContact_AgencyPhonebooks_AgencyPhonebookId",
                        column: x => x.AgencyPhonebookId,
                        principalTable: "AgencyPhonebooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "B2bCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LegalStatus = table.Column<int>(type: "int", nullable: false),
                    IsSupplier = table.Column<bool>(type: "bit", nullable: false),
                    IsSubcontractor = table.Column<bool>(type: "bit", nullable: false),
                    IsEngineering = table.Column<bool>(type: "bit", nullable: false),
                    IsEnterprise = table.Column<bool>(type: "bit", nullable: false),
                    TaxOffice = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AddedByAgencyId = table.Column<int>(type: "int", nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    Rating = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B2bCompanies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_B2bCompanies_Agencies_AddedByAgencyId",
                        column: x => x.AddedByAgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_B2bCompanies_AspNetUsers_AddedByUserId",
                        column: x => x.AddedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InstitutionRecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GooglePlaceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GoogleMapsUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionRecord", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModuleDocumentRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TargetModule = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    SystemLegalDocumentTemplateId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleDocumentRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleDocumentRules_SystemLegalDocumentTemplates_SystemLegalDocumentTemplateId",
                        column: x => x.SystemLegalDocumentTemplateId,
                        principalTable: "SystemLegalDocumentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "B2bBranches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    B2bCompanyId = table.Column<int>(type: "int", nullable: false),
                    BranchName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    GoogleMapsUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LandlinePhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B2bBranches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_B2bBranches_B2bCompanies_B2bCompanyId",
                        column: x => x.B2bCompanyId,
                        principalTable: "B2bCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "B2bCompanyCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    B2bCompanyId = table.Column<int>(type: "int", nullable: false),
                    DefinitionValueId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B2bCompanyCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_B2bCompanyCategories_B2bCompanies_B2bCompanyId",
                        column: x => x.B2bCompanyId,
                        principalTable: "B2bCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_B2bCompanyCategories_DefinitionValues_DefinitionValueId",
                        column: x => x.DefinitionValueId,
                        principalTable: "DefinitionValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "B2bContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    B2bCompanyId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartmentOrRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MobilePhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LandlinePhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExtensionNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_B2bContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_B2bContacts_B2bCompanies_B2bCompanyId",
                        column: x => x.B2bCompanyId,
                        principalTable: "B2bCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConstructionBudgetItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConstructionProjectId = table.Column<int>(type: "int", nullable: false),
                    PhaseCategory = table.Column<int>(type: "int", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    QuoteStatus = table.Column<int>(type: "int", nullable: false),
                    PlannedUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualTotalCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsUnplannedExtra = table.Column<bool>(type: "bit", nullable: false),
                    AssetDetailsId = table.Column<int>(type: "int", nullable: true),
                    SupplierId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConstructionBudgetItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConstructionBudgetItems_B2bCompanies_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "B2bCompanies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ConstructionBudgetItems_ConstructionProjects_ConstructionProjectId",
                        column: x => x.ConstructionProjectId,
                        principalTable: "ConstructionProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InstitutionContact",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyId = table.Column<int>(type: "int", nullable: false),
                    InstitutionRecordId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstitutionContact_Agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InstitutionContact_InstitutionRecord_InstitutionRecordId",
                        column: x => x.InstitutionRecordId,
                        principalTable: "InstitutionRecord",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModuleDocumentRulePrerequisites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleDocumentRuleId = table.Column<int>(type: "int", nullable: false),
                    PrerequisiteTemplateId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleDocumentRulePrerequisites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModuleDocumentRulePrerequisites_ModuleDocumentRules_ModuleDocumentRuleId",
                        column: x => x.ModuleDocumentRuleId,
                        principalTable: "ModuleDocumentRules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModuleDocumentRulePrerequisites_SystemLegalDocumentTemplates_PrerequisiteTemplateId",
                        column: x => x.PrerequisiteTemplateId,
                        principalTable: "SystemLegalDocumentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLegalDocuments_TargetContactId",
                table: "ProjectLegalDocuments",
                column: "TargetContactId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectLegalDocuments_TargetInstitutionId",
                table: "ProjectLegalDocuments",
                column: "TargetInstitutionId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyPhonebookBranch_AgencyPhonebookId",
                table: "AgencyPhonebookBranch",
                column: "AgencyPhonebookId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyPhonebookContact_AgencyPhonebookId",
                table: "AgencyPhonebookContact",
                column: "AgencyPhonebookId");

            migrationBuilder.CreateIndex(
                name: "IX_B2bBranches_B2bCompanyId",
                table: "B2bBranches",
                column: "B2bCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_B2bCompanies_AddedByAgencyId",
                table: "B2bCompanies",
                column: "AddedByAgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_B2bCompanies_AddedByUserId",
                table: "B2bCompanies",
                column: "AddedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_B2bCompanyCategories_B2bCompanyId",
                table: "B2bCompanyCategories",
                column: "B2bCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_B2bCompanyCategories_DefinitionValueId",
                table: "B2bCompanyCategories",
                column: "DefinitionValueId");

            migrationBuilder.CreateIndex(
                name: "IX_B2bContacts_B2bCompanyId",
                table: "B2bContacts",
                column: "B2bCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_ConstructionBudgetItems_ConstructionProjectId",
                table: "ConstructionBudgetItems",
                column: "ConstructionProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ConstructionBudgetItems_SupplierId",
                table: "ConstructionBudgetItems",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionContact_AgencyId",
                table: "InstitutionContact",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_InstitutionContact_InstitutionRecordId",
                table: "InstitutionContact",
                column: "InstitutionRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleDocumentRulePrerequisites_ModuleDocumentRuleId",
                table: "ModuleDocumentRulePrerequisites",
                column: "ModuleDocumentRuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleDocumentRulePrerequisites_PrerequisiteTemplateId",
                table: "ModuleDocumentRulePrerequisites",
                column: "PrerequisiteTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleDocumentRules_SystemLegalDocumentTemplateId",
                table: "ModuleDocumentRules",
                column: "SystemLegalDocumentTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectLegalDocuments_InstitutionContact_TargetContactId",
                table: "ProjectLegalDocuments",
                column: "TargetContactId",
                principalTable: "InstitutionContact",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectLegalDocuments_InstitutionRecord_TargetInstitutionId",
                table: "ProjectLegalDocuments",
                column: "TargetInstitutionId",
                principalTable: "InstitutionRecord",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectLegalDocuments_InstitutionContact_TargetContactId",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectLegalDocuments_InstitutionRecord_TargetInstitutionId",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropTable(
                name: "AgencyPhonebookBranch");

            migrationBuilder.DropTable(
                name: "AgencyPhonebookContact");

            migrationBuilder.DropTable(
                name: "B2bBranches");

            migrationBuilder.DropTable(
                name: "B2bCompanyCategories");

            migrationBuilder.DropTable(
                name: "B2bContacts");

            migrationBuilder.DropTable(
                name: "ConstructionBudgetItems");

            migrationBuilder.DropTable(
                name: "InstitutionContact");

            migrationBuilder.DropTable(
                name: "ModuleDocumentRulePrerequisites");

            migrationBuilder.DropTable(
                name: "B2bCompanies");

            migrationBuilder.DropTable(
                name: "InstitutionRecord");

            migrationBuilder.DropTable(
                name: "ModuleDocumentRules");

            migrationBuilder.DropIndex(
                name: "IX_ProjectLegalDocuments_TargetContactId",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropIndex(
                name: "IX_ProjectLegalDocuments_TargetInstitutionId",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropColumn(
                name: "AdditionalCost",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropColumn(
                name: "DocumentFee",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropColumn(
                name: "TargetContactId",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropColumn(
                name: "TargetInstitutionId",
                table: "ProjectLegalDocuments");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "AgencyPhonebooks");

            migrationBuilder.DropColumn(
                name: "AuthorizedPerson",
                table: "AgencyPhonebooks");

            migrationBuilder.DropColumn(
                name: "AuthorizedPersonRole",
                table: "AgencyPhonebooks");

            migrationBuilder.DropColumn(
                name: "City",
                table: "AgencyPhonebooks");

            migrationBuilder.DropColumn(
                name: "District",
                table: "AgencyPhonebooks");

            migrationBuilder.DropColumn(
                name: "ExtensionNumber",
                table: "AgencyPhonebooks");

            migrationBuilder.DropColumn(
                name: "GoogleMapsUrl",
                table: "AgencyPhonebooks");

            migrationBuilder.DropColumn(
                name: "Iban",
                table: "AgencyPhonebooks");

            migrationBuilder.DropColumn(
                name: "LandlinePhone",
                table: "AgencyPhonebooks");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "AgencyPhonebooks");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "AgencyPhonebooks");

            migrationBuilder.DropColumn(
                name: "MobilePhone2",
                table: "AgencyPhonebooks");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "AgencyPhonebooks");

            migrationBuilder.DropColumn(
                name: "TaxOffice",
                table: "AgencyPhonebooks");

            migrationBuilder.DropColumn(
                name: "WebsiteUrl",
                table: "AgencyPhonebooks");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "SystemLegalDocumentTemplates",
                newName: "Stage");

            migrationBuilder.AddColumn<bool>(
                name: "IsMandatory",
                table: "SystemLegalDocumentTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TargetModule",
                table: "SystemLegalDocumentTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ProjectLegalDocuments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "SystemDocumentDependency",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrerequisiteDocumentId = table.Column<int>(type: "int", nullable: false),
                    TargetDocumentId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemDocumentDependency", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SystemDocumentDependency_SystemLegalDocumentTemplates_PrerequisiteDocumentId",
                        column: x => x.PrerequisiteDocumentId,
                        principalTable: "SystemLegalDocumentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SystemDocumentDependency_SystemLegalDocumentTemplates_TargetDocumentId",
                        column: x => x.TargetDocumentId,
                        principalTable: "SystemLegalDocumentTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SystemDocumentDependency_PrerequisiteDocumentId",
                table: "SystemDocumentDependency",
                column: "PrerequisiteDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemDocumentDependency_TargetDocumentId",
                table: "SystemDocumentDependency",
                column: "TargetDocumentId");
        }
    }
}
