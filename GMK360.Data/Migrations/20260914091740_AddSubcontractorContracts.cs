using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubcontractorContracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubcontractorContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgencyId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    SubcontractorId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContractDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsExpenseContract = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubcontractorContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubcontractorContracts_Agencies_AgencyId",
                        column: x => x.AgencyId,
                        principalTable: "Agencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubcontractorContracts_B2BNetworkContacts_SubcontractorId",
                        column: x => x.SubcontractorId,
                        principalTable: "B2BNetworkContacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubcontractorContracts_ConstructionProjects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "ConstructionProjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractPhases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubcontractorContractId = table.Column<int>(type: "int", nullable: false),
                    PhaseName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractPhases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractPhases_SubcontractorContracts_SubcontractorContractId",
                        column: x => x.SubcontractorContractId,
                        principalTable: "SubcontractorContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgressPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubcontractorContractId = table.Column<int>(type: "int", nullable: false),
                    ContractPhaseId = table.Column<int>(type: "int", nullable: false),
                    PaymentTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RetentionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgressPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgressPayments_ContractPhases_ContractPhaseId",
                        column: x => x.ContractPhaseId,
                        principalTable: "ContractPhases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgressPayments_SubcontractorContracts_SubcontractorContractId",
                        column: x => x.SubcontractorContractId,
                        principalTable: "SubcontractorContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "AuthScreenBanners",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ActionText", "Subtitle", "Title" },
                values: new object[] { "Hemen ?lan Ver", "Yeni nesil emlak platformuna kat?larak ilanlar?n?z? milyonlara ula?t?r?n veya hayalinizdeki evi bulun.", "Sekt?r?n Zirvesine ??k?n." });

            migrationBuilder.UpdateData(
                table: "AuthScreenBanners",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ActionText", "Subtitle", "Title" },
                values: new object[] { "Kampanyaya Kat?l", "Bulundu?unuz ildeki ilk 3 kurumsal ?ye aras?na girin, 1 y?ll?k Premium Vitrin paketini an?nda kap?n.", "?lk 3 ?yeye ?zel F?rsat!" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 34,
                column: "Name",
                value: "?stanbul");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 35,
                column: "Name",
                value: "?zmir");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "T?rkiye");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Kad?k?y");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Be?ikta?");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "?i?li");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "?ankaya");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Ke?i?ren");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Kar??yaka");

            migrationBuilder.UpdateData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Kira Gelir Vergisi (GMS?)");

            migrationBuilder.UpdateData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "?evre Temizlik Vergisi (?TV)");

            migrationBuilder.UpdateData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "?lan ve Reklam Vergisi (Tabela)");

            migrationBuilder.UpdateData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Kira Stopaj? (Muhtasar Beyanname)");

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Ac?badem");

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Bostanc?");

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Bah?elievler");

            migrationBuilder.UpdateData(
                table: "Streets",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "G?l Sokak");

            migrationBuilder.CreateIndex(
                name: "IX_ContractPhases_SubcontractorContractId",
                table: "ContractPhases",
                column: "SubcontractorContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressPayments_ContractPhaseId",
                table: "ProgressPayments",
                column: "ContractPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgressPayments_SubcontractorContractId",
                table: "ProgressPayments",
                column: "SubcontractorContractId");

            migrationBuilder.CreateIndex(
                name: "IX_SubcontractorContracts_AgencyId",
                table: "SubcontractorContracts",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_SubcontractorContracts_ProjectId",
                table: "SubcontractorContracts",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_SubcontractorContracts_SubcontractorId",
                table: "SubcontractorContracts",
                column: "SubcontractorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProgressPayments");

            migrationBuilder.DropTable(
                name: "ContractPhases");

            migrationBuilder.DropTable(
                name: "SubcontractorContracts");

            migrationBuilder.UpdateData(
                table: "AuthScreenBanners",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ActionText", "Subtitle", "Title" },
                values: new object[] { "Hemen �lan Ver", "Yeni nesil emlak platformuna kat�larak ilanlar�n�z� milyonlara ula�t�r�n veya hayalinizdeki evi bulun.", "Sekt�r�n Zirvesine ��k�n." });

            migrationBuilder.UpdateData(
                table: "AuthScreenBanners",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ActionText", "Subtitle", "Title" },
                values: new object[] { "Kampanyaya Kat�l", "Bulundu�unuz ildeki ilk 3 kurumsal �ye aras�na girin, 1 y�ll�k Premium Vitrin paketini an�nda kap�n.", "�lk 3 �yeye �zel F�rsat!" });

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 34,
                column: "Name",
                value: "�stanbul");

            migrationBuilder.UpdateData(
                table: "Cities",
                keyColumn: "Id",
                keyValue: 35,
                column: "Name",
                value: "�zmir");

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "T�rkiye");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Kad�k�y");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Be�ikta�");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "�i�li");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "�ankaya");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Ke�i�ren");

            migrationBuilder.UpdateData(
                table: "Districts",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "Kar��yaka");

            migrationBuilder.UpdateData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Kira Gelir Vergisi (GMS�)");

            migrationBuilder.UpdateData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "�evre Temizlik Vergisi (�TV)");

            migrationBuilder.UpdateData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "�lan ve Reklam Vergisi (Tabela)");

            migrationBuilder.UpdateData(
                table: "FinancialObligationTypes",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "Kira Stopaj� (Muhtasar Beyanname)");

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Ac�badem");

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Bostanc�");

            migrationBuilder.UpdateData(
                table: "Neighborhoods",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Bah�elievler");

            migrationBuilder.UpdateData(
                table: "Streets",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "G�l Sokak");
        }
    }
}
