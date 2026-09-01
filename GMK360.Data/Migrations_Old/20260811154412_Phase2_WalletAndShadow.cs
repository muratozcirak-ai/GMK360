using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase2_WalletAndShadow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComplexBlocks_Complexes_ComplexId",
                table: "ComplexBlocks");

            migrationBuilder.DropForeignKey(
                name: "FK_ComplexFeatures_Complexes_ComplexId",
                table: "ComplexFeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Complexes_ComplexId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_ComplexId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "BlockName",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "BuildingNumber",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ComplexId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "HasBlock",
                table: "Properties");

            migrationBuilder.RenameColumn(
                name: "UnitNumber",
                table: "Properties",
                newName: "DoorNumber");

            migrationBuilder.RenameColumn(
                name: "TotalFloors",
                table: "Properties",
                newName: "ComplexId1");

            migrationBuilder.RenameColumn(
                name: "ComplexId",
                table: "ComplexBlocks",
                newName: "BuildingId");

            migrationBuilder.RenameIndex(
                name: "IX_ComplexBlocks_ComplexId",
                table: "ComplexBlocks",
                newName: "IX_ComplexBlocks_BuildingId");

            migrationBuilder.RenameColumn(
                name: "UnitNumber",
                table: "BuildingUnits",
                newName: "DoorNumber");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Buildings",
                newName: "Street");

            migrationBuilder.AddColumn<decimal>(
                name: "GrossAmount",
                table: "UserWalletTransactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "RelatedPropertyId",
                table: "UserWalletTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "UserWalletTransactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TaxFundType",
                table: "UserWalletTransactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuildingId",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ComplexId1",
                table: "ComplexFeatures",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ComplexId1",
                table: "ComplexBlocks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BlockName",
                table: "Buildings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BuildingNumber",
                table: "Buildings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "Buildings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "Buildings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasBlock",
                table: "Buildings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasBlocks",
                table: "Buildings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Buildings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Buildings",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Buildings",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NeighborhoodId",
                table: "Buildings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StreetId",
                table: "Buildings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalFloors",
                table: "Buildings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShadowCreatorId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PropertyUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleType = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ModuleType = table.Column<int>(type: "int", nullable: false),
                    ReferenceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UploadedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_UserWalletTransactions_RelatedPropertyId",
                table: "UserWalletTransactions",
                column: "RelatedPropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_BuildingId",
                table: "Properties",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_ComplexId1",
                table: "Properties",
                column: "ComplexId1");

            migrationBuilder.CreateIndex(
                name: "IX_ComplexFeatures_ComplexId1",
                table: "ComplexFeatures",
                column: "ComplexId1");

            migrationBuilder.CreateIndex(
                name: "IX_ComplexBlocks_ComplexId1",
                table: "ComplexBlocks",
                column: "ComplexId1");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_CityId",
                table: "Buildings",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_DistrictId",
                table: "Buildings",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_NeighborhoodId",
                table: "Buildings",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ShadowCreatorId",
                table: "AspNetUsers",
                column: "ShadowCreatorId");

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
                name: "FK_Buildings_Cities_CityId",
                table: "Buildings",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_Districts_DistrictId",
                table: "Buildings",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_Neighborhoods_NeighborhoodId",
                table: "Buildings",
                column: "NeighborhoodId",
                principalTable: "Neighborhoods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComplexBlocks_Buildings_BuildingId",
                table: "ComplexBlocks",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ComplexBlocks_Complexes_ComplexId1",
                table: "ComplexBlocks",
                column: "ComplexId1",
                principalTable: "Complexes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplexFeatures_Buildings_ComplexId",
                table: "ComplexFeatures",
                column: "ComplexId",
                principalTable: "Buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ComplexFeatures_Complexes_ComplexId1",
                table: "ComplexFeatures",
                column: "ComplexId1",
                principalTable: "Complexes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Buildings_BuildingId",
                table: "Properties",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Complexes_ComplexId1",
                table: "Properties",
                column: "ComplexId1",
                principalTable: "Complexes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserWalletTransactions_Properties_RelatedPropertyId",
                table: "UserWalletTransactions",
                column: "RelatedPropertyId",
                principalTable: "Properties",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_ShadowCreatorId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_Cities_CityId",
                table: "Buildings");

            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_Districts_DistrictId",
                table: "Buildings");

            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_Neighborhoods_NeighborhoodId",
                table: "Buildings");

            migrationBuilder.DropForeignKey(
                name: "FK_ComplexBlocks_Buildings_BuildingId",
                table: "ComplexBlocks");

            migrationBuilder.DropForeignKey(
                name: "FK_ComplexBlocks_Complexes_ComplexId1",
                table: "ComplexBlocks");

            migrationBuilder.DropForeignKey(
                name: "FK_ComplexFeatures_Buildings_ComplexId",
                table: "ComplexFeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_ComplexFeatures_Complexes_ComplexId1",
                table: "ComplexFeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Buildings_BuildingId",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Complexes_ComplexId1",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_UserWalletTransactions_Properties_RelatedPropertyId",
                table: "UserWalletTransactions");

            migrationBuilder.DropTable(
                name: "PropertyUsers");

            migrationBuilder.DropTable(
                name: "SystemDocuments");

            migrationBuilder.DropIndex(
                name: "IX_UserWalletTransactions_RelatedPropertyId",
                table: "UserWalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Properties_BuildingId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_ComplexId1",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_ComplexFeatures_ComplexId1",
                table: "ComplexFeatures");

            migrationBuilder.DropIndex(
                name: "IX_ComplexBlocks_ComplexId1",
                table: "ComplexBlocks");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_CityId",
                table: "Buildings");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_DistrictId",
                table: "Buildings");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_NeighborhoodId",
                table: "Buildings");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ShadowCreatorId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GrossAmount",
                table: "UserWalletTransactions");

            migrationBuilder.DropColumn(
                name: "RelatedPropertyId",
                table: "UserWalletTransactions");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "UserWalletTransactions");

            migrationBuilder.DropColumn(
                name: "TaxFundType",
                table: "UserWalletTransactions");

            migrationBuilder.DropColumn(
                name: "BuildingId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "ComplexId1",
                table: "ComplexFeatures");

            migrationBuilder.DropColumn(
                name: "ComplexId1",
                table: "ComplexBlocks");

            migrationBuilder.DropColumn(
                name: "BlockName",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "BuildingNumber",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "HasBlock",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "HasBlocks",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "NeighborhoodId",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "StreetId",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "TotalFloors",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "ShadowCreatorId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "DoorNumber",
                table: "Properties",
                newName: "UnitNumber");

            migrationBuilder.RenameColumn(
                name: "ComplexId1",
                table: "Properties",
                newName: "TotalFloors");

            migrationBuilder.RenameColumn(
                name: "BuildingId",
                table: "ComplexBlocks",
                newName: "ComplexId");

            migrationBuilder.RenameIndex(
                name: "IX_ComplexBlocks_BuildingId",
                table: "ComplexBlocks",
                newName: "IX_ComplexBlocks_ComplexId");

            migrationBuilder.RenameColumn(
                name: "DoorNumber",
                table: "BuildingUnits",
                newName: "UnitNumber");

            migrationBuilder.RenameColumn(
                name: "Street",
                table: "Buildings",
                newName: "Address");

            migrationBuilder.AddColumn<string>(
                name: "BlockName",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BuildingNumber",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ComplexId",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasBlock",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_ComplexId",
                table: "Properties",
                column: "ComplexId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplexBlocks_Complexes_ComplexId",
                table: "ComplexBlocks",
                column: "ComplexId",
                principalTable: "Complexes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ComplexFeatures_Complexes_ComplexId",
                table: "ComplexFeatures",
                column: "ComplexId",
                principalTable: "Complexes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Complexes_ComplexId",
                table: "Properties",
                column: "ComplexId",
                principalTable: "Complexes",
                principalColumn: "Id");
        }
    }
}
