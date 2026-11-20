using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFacilityResidencyAndApprovals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UnitMaterialSelections_ProjectMaterialCatalogs_ProjectMaterialCatalogId",
                table: "UnitMaterialSelections");

            migrationBuilder.AddColumn<string>(
                name: "DumpSiteName",
                table: "VehicleTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EndWorkHour",
                table: "VehicleTasks",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FuelLiters",
                table: "VehicleTasks",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialType",
                table: "VehicleTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "StartWorkHour",
                table: "VehicleTasks",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaskCategory",
                table: "VehicleTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TicketNumber",
                table: "VehicleTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TicketPhotoPath",
                table: "VehicleTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Tonnage",
                table: "VehicleTasks",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TripNumber",
                table: "VehicleTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SelectedByUserId",
                table: "UnitMaterialSelections",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectMaterialCatalogId",
                table: "UnitMaterialSelections",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "ApprovedPriceDifference",
                table: "UnitMaterialSelections",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "UnitMaterialSelections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmedDate",
                table: "UnitMaterialSelections",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ConfirmedTime",
                table: "UnitMaterialSelections",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomRequestImageUrl",
                table: "UnitMaterialSelections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomRequestNotes",
                table: "UnitMaterialSelections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmedViaSms",
                table: "UnitMaterialSelections",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OtpCode",
                table: "UnitMaterialSelections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectionStatus",
                table: "UnitMaterialSelections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShowroomProductCode",
                table: "UnitMaterialSelections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShowroomSupplierName",
                table: "UnitMaterialSelections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationPhone",
                table: "UnitMaterialSelections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "UnitDebts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PayerRole",
                table: "UnitDebts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ResidencyPeriodId",
                table: "UnitDebts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReversalOfDebtId",
                table: "UnitDebts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialSelectionNotes",
                table: "SpaceMeasurements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PriceDifference",
                table: "SpaceMeasurements",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedMaterialOption",
                table: "SpaceMeasurements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "ProjectStakeholders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Iban",
                table: "ProjectStakeholders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPaymentDate",
                table: "ProjectStakeholders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyRentAllowance",
                table: "ProjectStakeholders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentDay",
                table: "ProjectStakeholders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                table: "ProjectStakeholders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptUrl",
                table: "ProjectStakeholders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RelocationAllowance",
                table: "ProjectStakeholders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RentEndDate",
                table: "ProjectStakeholders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RentStartDate",
                table: "ProjectStakeholders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShareRatio",
                table: "ProjectStakeholders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitCount",
                table: "ProjectStakeholders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitNumbers",
                table: "ProjectStakeholders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FloorOrZone",
                table: "ProjectScheduleTasks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "BuildingId",
                table: "ProjectScheduleTasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaterialTitle",
                table: "ProjectScheduleTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartCategory",
                table: "ProjectScheduleTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartIndex",
                table: "ProjectScheduleTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlannedCrewSize",
                table: "ProjectScheduleTasks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProfessionName",
                table: "ProjectScheduleTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "ProjectScheduleTasks",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "ProjectScheduleTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletedBy",
                table: "ProjectScheduleSubTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedDate",
                table: "ProjectScheduleSubTasks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentUrl",
                table: "ProjectScheduleSubTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "ProjectScheduleSubTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "ProjectScheduleSubTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestResultValue",
                table: "ProjectScheduleSubTasks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "DxfFilePath",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasDxfDrawing",
                table: "ConstructionProjects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsJointVenture",
                table: "ConstructionProjects",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ManagerApprovalPin",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OurSharePercentage",
                table: "ConstructionProjects",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OurVaultAdvance",
                table: "ConstructionProjects",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParcelPolygonGeoJson",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentProjectId",
                table: "ConstructionProjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerCompanyName",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerContactPerson",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerContractDocumentPath",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerEmail",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerPhone",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PartnerSharePercentage",
                table: "ConstructionProjects",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PartnerVaultAdvance",
                table: "ConstructionProjects",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnersJson",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProjectExecutionMode",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RelatedAmenityId",
                table: "ConstructionProjects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RentPolicyJson",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubProjectCategory",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AmountExcludingVat",
                table: "ConstructionProjectExpenses",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalPin",
                table: "ConstructionProjectExpenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "ConstructionProjectExpenses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalType",
                table: "ConstructionProjectExpenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "ConstructionProjectExpenses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "ConstructionProjectExpenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "ConstructionProjectExpenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrencyAmount",
                table: "ConstructionProjectExpenses",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "ConstructionProjectExpenses",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsJointVenture",
                table: "ConstructionProjectExpenses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "OurShareAmount",
                table: "ConstructionProjectExpenses",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OurSharePercentage",
                table: "ConstructionProjectExpenses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PartnerCompanyName",
                table: "ConstructionProjectExpenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PartnerShareAmount",
                table: "ConstructionProjectExpenses",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PartnerSharePercentage",
                table: "ConstructionProjectExpenses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PayeeOrSupplier",
                table: "ConstructionProjectExpenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentSource",
                table: "ConstructionProjectExpenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SiteDiaryDate",
                table: "ConstructionProjectExpenses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiteDiaryNote",
                table: "ConstructionProjectExpenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VatAmount",
                table: "ConstructionProjectExpenses",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VatRate",
                table: "ConstructionProjectExpenses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "InspectionExpiryDate",
                table: "CompanyVehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceCompany",
                table: "CompanyVehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InsuranceExpiryDate",
                table: "CompanyVehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsurancePolicyNo",
                table: "CompanyVehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsurancePolicyPhotoPath",
                table: "CompanyVehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationPhotoPath",
                table: "CompanyVehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegistrationSerialNo",
                table: "CompanyVehicles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "BuildingExpenses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArchivedByUserId",
                table: "BuildingExpenses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "BuildingExpenses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReversalOfExpenseId",
                table: "BuildingExpenses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApprovalRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingId = table.Column<int>(type: "int", nullable: false),
                    RequestedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    TargetEntityName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    TargetEntityId = table.Column<int>(type: "int", nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequiredApprovals = table.Column<int>(type: "int", nullable: false),
                    ApprovalCount = table.Column<int>(type: "int", nullable: false),
                    ApprovedByJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalRequests_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ResidencyPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingUnitId = table.Column<int>(type: "int", nullable: false),
                    ResidentUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RoleType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ArchiveNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VotingPinHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ProxyUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ProxyDocumentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResidencyPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResidencyPeriods_AspNetUsers_ResidentUserId",
                        column: x => x.ResidentUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ResidencyPeriods_BuildingUnits_BuildingUnitId",
                        column: x => x.BuildingUnitId,
                        principalTable: "BuildingUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnitDebts_ResidencyPeriodId",
                table: "UnitDebts",
                column: "ResidencyPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_ConstructionProjects_ParentProjectId",
                table: "ConstructionProjects",
                column: "ParentProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ConstructionProjects_RelatedAmenityId",
                table: "ConstructionProjects",
                column: "RelatedAmenityId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalRequests_BuildingId",
                table: "ApprovalRequests",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_ResidencyPeriods_BuildingUnitId",
                table: "ResidencyPeriods",
                column: "BuildingUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ResidencyPeriods_ResidentUserId",
                table: "ResidencyPeriods",
                column: "ResidentUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConstructionProjects_ConstructionProjects_ParentProjectId",
                table: "ConstructionProjects",
                column: "ParentProjectId",
                principalTable: "ConstructionProjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConstructionProjects_ProjectAmenities_RelatedAmenityId",
                table: "ConstructionProjects",
                column: "RelatedAmenityId",
                principalTable: "ProjectAmenities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UnitDebts_ResidencyPeriods_ResidencyPeriodId",
                table: "UnitDebts",
                column: "ResidencyPeriodId",
                principalTable: "ResidencyPeriods",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UnitMaterialSelections_ProjectMaterialCatalogs_ProjectMaterialCatalogId",
                table: "UnitMaterialSelections",
                column: "ProjectMaterialCatalogId",
                principalTable: "ProjectMaterialCatalogs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConstructionProjects_ConstructionProjects_ParentProjectId",
                table: "ConstructionProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_ConstructionProjects_ProjectAmenities_RelatedAmenityId",
                table: "ConstructionProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_UnitDebts_ResidencyPeriods_ResidencyPeriodId",
                table: "UnitDebts");

            migrationBuilder.DropForeignKey(
                name: "FK_UnitMaterialSelections_ProjectMaterialCatalogs_ProjectMaterialCatalogId",
                table: "UnitMaterialSelections");

            migrationBuilder.DropTable(
                name: "ApprovalRequests");

            migrationBuilder.DropTable(
                name: "ResidencyPeriods");

            migrationBuilder.DropIndex(
                name: "IX_UnitDebts_ResidencyPeriodId",
                table: "UnitDebts");

            migrationBuilder.DropIndex(
                name: "IX_ConstructionProjects_ParentProjectId",
                table: "ConstructionProjects");

            migrationBuilder.DropIndex(
                name: "IX_ConstructionProjects_RelatedAmenityId",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "DumpSiteName",
                table: "VehicleTasks");

            migrationBuilder.DropColumn(
                name: "EndWorkHour",
                table: "VehicleTasks");

            migrationBuilder.DropColumn(
                name: "FuelLiters",
                table: "VehicleTasks");

            migrationBuilder.DropColumn(
                name: "MaterialType",
                table: "VehicleTasks");

            migrationBuilder.DropColumn(
                name: "StartWorkHour",
                table: "VehicleTasks");

            migrationBuilder.DropColumn(
                name: "TaskCategory",
                table: "VehicleTasks");

            migrationBuilder.DropColumn(
                name: "TicketNumber",
                table: "VehicleTasks");

            migrationBuilder.DropColumn(
                name: "TicketPhotoPath",
                table: "VehicleTasks");

            migrationBuilder.DropColumn(
                name: "Tonnage",
                table: "VehicleTasks");

            migrationBuilder.DropColumn(
                name: "TripNumber",
                table: "VehicleTasks");

            migrationBuilder.DropColumn(
                name: "ApprovedPriceDifference",
                table: "UnitMaterialSelections");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "UnitMaterialSelections");

            migrationBuilder.DropColumn(
                name: "ConfirmedDate",
                table: "UnitMaterialSelections");

            migrationBuilder.DropColumn(
                name: "ConfirmedTime",
                table: "UnitMaterialSelections");

            migrationBuilder.DropColumn(
                name: "CustomRequestImageUrl",
                table: "UnitMaterialSelections");

            migrationBuilder.DropColumn(
                name: "CustomRequestNotes",
                table: "UnitMaterialSelections");

            migrationBuilder.DropColumn(
                name: "IsConfirmedViaSms",
                table: "UnitMaterialSelections");

            migrationBuilder.DropColumn(
                name: "OtpCode",
                table: "UnitMaterialSelections");

            migrationBuilder.DropColumn(
                name: "SelectionStatus",
                table: "UnitMaterialSelections");

            migrationBuilder.DropColumn(
                name: "ShowroomProductCode",
                table: "UnitMaterialSelections");

            migrationBuilder.DropColumn(
                name: "ShowroomSupplierName",
                table: "UnitMaterialSelections");

            migrationBuilder.DropColumn(
                name: "VerificationPhone",
                table: "UnitMaterialSelections");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "UnitDebts");

            migrationBuilder.DropColumn(
                name: "PayerRole",
                table: "UnitDebts");

            migrationBuilder.DropColumn(
                name: "ResidencyPeriodId",
                table: "UnitDebts");

            migrationBuilder.DropColumn(
                name: "ReversalOfDebtId",
                table: "UnitDebts");

            migrationBuilder.DropColumn(
                name: "MaterialSelectionNotes",
                table: "SpaceMeasurements");

            migrationBuilder.DropColumn(
                name: "PriceDifference",
                table: "SpaceMeasurements");

            migrationBuilder.DropColumn(
                name: "SelectedMaterialOption",
                table: "SpaceMeasurements");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "ProjectStakeholders");

            migrationBuilder.DropColumn(
                name: "Iban",
                table: "ProjectStakeholders");

            migrationBuilder.DropColumn(
                name: "LastPaymentDate",
                table: "ProjectStakeholders");

            migrationBuilder.DropColumn(
                name: "MonthlyRentAllowance",
                table: "ProjectStakeholders");

            migrationBuilder.DropColumn(
                name: "PaymentDay",
                table: "ProjectStakeholders");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "ProjectStakeholders");

            migrationBuilder.DropColumn(
                name: "ReceiptUrl",
                table: "ProjectStakeholders");

            migrationBuilder.DropColumn(
                name: "RelocationAllowance",
                table: "ProjectStakeholders");

            migrationBuilder.DropColumn(
                name: "RentEndDate",
                table: "ProjectStakeholders");

            migrationBuilder.DropColumn(
                name: "RentStartDate",
                table: "ProjectStakeholders");

            migrationBuilder.DropColumn(
                name: "ShareRatio",
                table: "ProjectStakeholders");

            migrationBuilder.DropColumn(
                name: "UnitCount",
                table: "ProjectStakeholders");

            migrationBuilder.DropColumn(
                name: "UnitNumbers",
                table: "ProjectStakeholders");

            migrationBuilder.DropColumn(
                name: "BuildingId",
                table: "ProjectScheduleTasks");

            migrationBuilder.DropColumn(
                name: "MaterialTitle",
                table: "ProjectScheduleTasks");

            migrationBuilder.DropColumn(
                name: "PartCategory",
                table: "ProjectScheduleTasks");

            migrationBuilder.DropColumn(
                name: "PartIndex",
                table: "ProjectScheduleTasks");

            migrationBuilder.DropColumn(
                name: "PlannedCrewSize",
                table: "ProjectScheduleTasks");

            migrationBuilder.DropColumn(
                name: "ProfessionName",
                table: "ProjectScheduleTasks");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ProjectScheduleTasks");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "ProjectScheduleTasks");

            migrationBuilder.DropColumn(
                name: "CompletedBy",
                table: "ProjectScheduleSubTasks");

            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "ProjectScheduleSubTasks");

            migrationBuilder.DropColumn(
                name: "DocumentUrl",
                table: "ProjectScheduleSubTasks");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "ProjectScheduleSubTasks");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "ProjectScheduleSubTasks");

            migrationBuilder.DropColumn(
                name: "TestResultValue",
                table: "ProjectScheduleSubTasks");

            migrationBuilder.DropColumn(
                name: "DxfFilePath",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "HasDxfDrawing",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "IsJointVenture",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "ManagerApprovalPin",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "OurSharePercentage",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "OurVaultAdvance",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "ParcelPolygonGeoJson",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "ParentProjectId",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "PartnerCompanyName",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "PartnerContactPerson",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "PartnerContractDocumentPath",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "PartnerEmail",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "PartnerPhone",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "PartnerSharePercentage",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "PartnerVaultAdvance",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "PartnersJson",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "ProjectExecutionMode",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "RelatedAmenityId",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "RentPolicyJson",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "SubProjectCategory",
                table: "ConstructionProjects");

            migrationBuilder.DropColumn(
                name: "AmountExcludingVat",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "ApprovalPin",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "ApprovalType",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "CurrencyAmount",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "IsJointVenture",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "OurShareAmount",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "OurSharePercentage",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "PartnerCompanyName",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "PartnerShareAmount",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "PartnerSharePercentage",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "PayeeOrSupplier",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "PaymentSource",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "SiteDiaryDate",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "SiteDiaryNote",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "VatAmount",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "ConstructionProjectExpenses");

            migrationBuilder.DropColumn(
                name: "InspectionExpiryDate",
                table: "CompanyVehicles");

            migrationBuilder.DropColumn(
                name: "InsuranceCompany",
                table: "CompanyVehicles");

            migrationBuilder.DropColumn(
                name: "InsuranceExpiryDate",
                table: "CompanyVehicles");

            migrationBuilder.DropColumn(
                name: "InsurancePolicyNo",
                table: "CompanyVehicles");

            migrationBuilder.DropColumn(
                name: "InsurancePolicyPhotoPath",
                table: "CompanyVehicles");

            migrationBuilder.DropColumn(
                name: "RegistrationPhotoPath",
                table: "CompanyVehicles");

            migrationBuilder.DropColumn(
                name: "RegistrationSerialNo",
                table: "CompanyVehicles");

            migrationBuilder.DropColumn(
                name: "ArchivedAt",
                table: "BuildingExpenses");

            migrationBuilder.DropColumn(
                name: "ArchivedByUserId",
                table: "BuildingExpenses");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "BuildingExpenses");

            migrationBuilder.DropColumn(
                name: "ReversalOfExpenseId",
                table: "BuildingExpenses");

            migrationBuilder.AlterColumn<string>(
                name: "SelectedByUserId",
                table: "UnitMaterialSelections",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ProjectMaterialCatalogId",
                table: "UnitMaterialSelections",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FloorOrZone",
                table: "ProjectScheduleTasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ConstructionProjects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_UnitMaterialSelections_ProjectMaterialCatalogs_ProjectMaterialCatalogId",
                table: "UnitMaterialSelections",
                column: "ProjectMaterialCatalogId",
                principalTable: "ProjectMaterialCatalogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
