using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketplaceAndShortTermRental : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UnitType",
                table: "BuildingUnits",
                newName: "RoomLayout");

            migrationBuilder.AddColumn<int>(
                name: "RenovationRequestId",
                table: "tbl_gmk_DijitalSozlesmeler",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedSupplierId",
                table: "RenovationRequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssigneeUserId",
                table: "RenovationRequests",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsManualJob",
                table: "RenovationRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Classification",
                table: "BuildingUnits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableForDailyRent",
                table: "BuildingUnits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "FinancialAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    ManagementCompanyId = table.Column<int>(type: "int", nullable: true),
                    AccountType = table.Column<int>(type: "int", nullable: false),
                    IBAN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubMerchantKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TaxOffice = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialAccounts_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FinancialAccounts_ManagementCompanies_ManagementCompanyId",
                        column: x => x.ManagementCompanyId,
                        principalTable: "ManagementCompanies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InvitationTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetEmailOrPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InviterUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TargetRole = table.Column<int>(type: "int", nullable: false),
                    RelatedUnitId = table.Column<int>(type: "int", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvitationTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvitationTokens_AspNetUsers_InviterUserId",
                        column: x => x.InviterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvitationTokens_BuildingUnits_RelatedUnitId",
                        column: x => x.RelatedUnitId,
                        principalTable: "BuildingUnits",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MaterialPriceInquiries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TradesmanUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SupplierUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaterialDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuotedPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialPriceInquiries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialPriceInquiries_AspNetUsers_SupplierUserId",
                        column: x => x.SupplierUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MaterialPriceInquiries_AspNetUsers_TradesmanUserId",
                        column: x => x.TradesmanUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlatformCommissionRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContextType = table.Column<int>(type: "int", nullable: false),
                    Percentage = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FixedFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SpecificUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformCommissionRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformCommissionRates_AspNetUsers_SpecificUserId",
                        column: x => x.SpecificUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Quotations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RenovationRequestId = table.Column<int>(type: "int", nullable: false),
                    TradesmanUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LaborCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaterialCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quotations_AspNetUsers_TradesmanUserId",
                        column: x => x.TradesmanUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Quotations_RenovationRequests_RenovationRequestId",
                        column: x => x.RenovationRequestId,
                        principalTable: "RenovationRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GuestCount = table.Column<int>(type: "int", nullable: false),
                    ReservationStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_BuildingUnits_UnitId",
                        column: x => x.UnitId,
                        principalTable: "BuildingUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierCampaigns_AspNetUsers_SupplierUserId",
                        column: x => x.SupplierUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierTradesmanRelations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TradesmanUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierTradesmanRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierTradesmanRelations_AspNetUsers_SupplierUserId",
                        column: x => x.SupplierUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupplierTradesmanRelations_AspNetUsers_TradesmanUserId",
                        column: x => x.TradesmanUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UniversalSurveys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TargetUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SenderUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RenovationRequestId = table.Column<int>(type: "int", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsApprovedForPublic = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniversalSurveys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UniversalSurveys_AspNetUsers_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UniversalSurveys_AspNetUsers_TargetUserId",
                        column: x => x.TargetUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UniversalSurveys_RenovationRequests_RenovationRequestId",
                        column: x => x.RenovationRequestId,
                        principalTable: "RenovationRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReceiverAccountId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PlatformCommissionAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetReceiverAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ContextType = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_AspNetUsers_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_FinancialAccounts_ReceiverAccountId",
                        column: x => x.ReceiverAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuestIdentities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TCIdentityNumberOrPassport = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsReportedToKbs = table.Column<bool>(type: "bit", nullable: false),
                    KbsReportDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestIdentities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestIdentities_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_gmk_DijitalSozlesmeler_RenovationRequestId",
                table: "tbl_gmk_DijitalSozlesmeler",
                column: "RenovationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RenovationRequests_AssignedSupplierId",
                table: "RenovationRequests",
                column: "AssignedSupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_RenovationRequests_AssigneeUserId",
                table: "RenovationRequests",
                column: "AssigneeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_AppUserId",
                table: "FinancialAccounts",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_ManagementCompanyId",
                table: "FinancialAccounts",
                column: "ManagementCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestIdentities_ReservationId",
                table: "GuestIdentities",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_InvitationTokens_InviterUserId",
                table: "InvitationTokens",
                column: "InviterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InvitationTokens_RelatedUnitId",
                table: "InvitationTokens",
                column: "RelatedUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialPriceInquiries_SupplierUserId",
                table: "MaterialPriceInquiries",
                column: "SupplierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialPriceInquiries_TradesmanUserId",
                table: "MaterialPriceInquiries",
                column: "TradesmanUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ReceiverAccountId",
                table: "PaymentTransactions",
                column: "ReceiverAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_SenderUserId",
                table: "PaymentTransactions",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformCommissionRates_SpecificUserId",
                table: "PlatformCommissionRates",
                column: "SpecificUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_RenovationRequestId",
                table: "Quotations",
                column: "RenovationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Quotations_TradesmanUserId",
                table: "Quotations",
                column: "TradesmanUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_UnitId",
                table: "Reservations",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCampaigns_SupplierUserId",
                table: "SupplierCampaigns",
                column: "SupplierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierTradesmanRelations_SupplierUserId",
                table: "SupplierTradesmanRelations",
                column: "SupplierUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierTradesmanRelations_TradesmanUserId",
                table: "SupplierTradesmanRelations",
                column: "TradesmanUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UniversalSurveys_RenovationRequestId",
                table: "UniversalSurveys",
                column: "RenovationRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_UniversalSurveys_SenderUserId",
                table: "UniversalSurveys",
                column: "SenderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UniversalSurveys_TargetUserId",
                table: "UniversalSurveys",
                column: "TargetUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RenovationRequests_AspNetUsers_AssignedSupplierId",
                table: "RenovationRequests",
                column: "AssignedSupplierId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RenovationRequests_AspNetUsers_AssigneeUserId",
                table: "RenovationRequests",
                column: "AssigneeUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_gmk_DijitalSozlesmeler_RenovationRequests_RenovationRequestId",
                table: "tbl_gmk_DijitalSozlesmeler",
                column: "RenovationRequestId",
                principalTable: "RenovationRequests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RenovationRequests_AspNetUsers_AssignedSupplierId",
                table: "RenovationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_RenovationRequests_AspNetUsers_AssigneeUserId",
                table: "RenovationRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_tbl_gmk_DijitalSozlesmeler_RenovationRequests_RenovationRequestId",
                table: "tbl_gmk_DijitalSozlesmeler");

            migrationBuilder.DropTable(
                name: "GuestIdentities");

            migrationBuilder.DropTable(
                name: "InvitationTokens");

            migrationBuilder.DropTable(
                name: "MaterialPriceInquiries");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "PlatformCommissionRates");

            migrationBuilder.DropTable(
                name: "Quotations");

            migrationBuilder.DropTable(
                name: "SupplierCampaigns");

            migrationBuilder.DropTable(
                name: "SupplierTradesmanRelations");

            migrationBuilder.DropTable(
                name: "UniversalSurveys");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "FinancialAccounts");

            migrationBuilder.DropIndex(
                name: "IX_tbl_gmk_DijitalSozlesmeler_RenovationRequestId",
                table: "tbl_gmk_DijitalSozlesmeler");

            migrationBuilder.DropIndex(
                name: "IX_RenovationRequests_AssignedSupplierId",
                table: "RenovationRequests");

            migrationBuilder.DropIndex(
                name: "IX_RenovationRequests_AssigneeUserId",
                table: "RenovationRequests");

            migrationBuilder.DropColumn(
                name: "RenovationRequestId",
                table: "tbl_gmk_DijitalSozlesmeler");

            migrationBuilder.DropColumn(
                name: "AssignedSupplierId",
                table: "RenovationRequests");

            migrationBuilder.DropColumn(
                name: "AssigneeUserId",
                table: "RenovationRequests");

            migrationBuilder.DropColumn(
                name: "IsManualJob",
                table: "RenovationRequests");

            migrationBuilder.DropColumn(
                name: "Classification",
                table: "BuildingUnits");

            migrationBuilder.DropColumn(
                name: "IsAvailableForDailyRent",
                table: "BuildingUnits");

            migrationBuilder.RenameColumn(
                name: "RoomLayout",
                table: "BuildingUnits",
                newName: "UnitType");
        }
    }
}
