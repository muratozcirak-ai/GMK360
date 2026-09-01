using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class Add_Copilot_And_Architecture_Fixes_V4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserFavorites_UserId",
                table: "UserFavorites");

            migrationBuilder.DropIndex(
                name: "IX_Properties_BuildingId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_UserId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_CityId",
                table: "Buildings");

            migrationBuilder.RenameIndex(
                name: "IX_PropertyImages_PropertyId",
                table: "PropertyImages",
                newName: "IX_PropertyImage_Property");

            migrationBuilder.RenameColumn(
                name: "Street",
                table: "Buildings",
                newName: "StreetName");

            migrationBuilder.AlterColumn<int>(
                name: "ManagementCompanyId",
                table: "HousingComplexes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "HousingComplexes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "HousingComplexes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NeighborhoodId",
                table: "HousingComplexes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StreetId",
                table: "HousingComplexes",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UsdRate",
                table: "ExchangeRates",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "EurRate",
                table: "ExchangeRates",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<int>(
                name: "HousingComplexId",
                table: "ComplexFeatures",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserFavorite_User_Created",
                table: "UserFavorites",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PropertyImage_Property_Sort",
                table: "PropertyImages",
                columns: new[] { "PropertyId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Property_Building",
                table: "Properties",
                columns: new[] { "BuildingId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Property_Price_Status",
                table: "Properties",
                columns: new[] { "Price", "StatusId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Property_Status_Created",
                table: "Properties",
                columns: new[] { "IsDeleted", "StatusId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Property_User",
                table: "Properties",
                columns: new[] { "UserId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_HousingComplexes_CityId",
                table: "HousingComplexes",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingComplexes_DistrictId",
                table: "HousingComplexes",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingComplexes_NeighborhoodId",
                table: "HousingComplexes",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_HousingComplexes_StreetId",
                table: "HousingComplexes",
                column: "StreetId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplexFeatures_HousingComplexId",
                table: "ComplexFeatures",
                column: "HousingComplexId");

            migrationBuilder.CreateIndex(
                name: "IX_Building_Location",
                table: "Buildings",
                columns: new[] { "CityId", "DistrictId", "NeighborhoodId" });

            migrationBuilder.CreateIndex(
                name: "IX_Building_Street",
                table: "Buildings",
                column: "StreetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_Streets_StreetId",
                table: "Buildings",
                column: "StreetId",
                principalTable: "Streets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComplexFeatures_HousingComplexes_HousingComplexId",
                table: "ComplexFeatures",
                column: "HousingComplexId",
                principalTable: "HousingComplexes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HousingComplexes_Cities_CityId",
                table: "HousingComplexes",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HousingComplexes_Districts_DistrictId",
                table: "HousingComplexes",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HousingComplexes_Neighborhoods_NeighborhoodId",
                table: "HousingComplexes",
                column: "NeighborhoodId",
                principalTable: "Neighborhoods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HousingComplexes_Streets_StreetId",
                table: "HousingComplexes",
                column: "StreetId",
                principalTable: "Streets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_Streets_StreetId",
                table: "Buildings");

            migrationBuilder.DropForeignKey(
                name: "FK_ComplexFeatures_HousingComplexes_HousingComplexId",
                table: "ComplexFeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingComplexes_Cities_CityId",
                table: "HousingComplexes");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingComplexes_Districts_DistrictId",
                table: "HousingComplexes");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingComplexes_Neighborhoods_NeighborhoodId",
                table: "HousingComplexes");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingComplexes_Streets_StreetId",
                table: "HousingComplexes");

            migrationBuilder.DropIndex(
                name: "IX_UserFavorite_User_Created",
                table: "UserFavorites");

            migrationBuilder.DropIndex(
                name: "IX_PropertyImage_Property_Sort",
                table: "PropertyImages");

            migrationBuilder.DropIndex(
                name: "IX_Property_Building",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Property_Price_Status",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Property_Status_Created",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Property_User",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_HousingComplexes_CityId",
                table: "HousingComplexes");

            migrationBuilder.DropIndex(
                name: "IX_HousingComplexes_DistrictId",
                table: "HousingComplexes");

            migrationBuilder.DropIndex(
                name: "IX_HousingComplexes_NeighborhoodId",
                table: "HousingComplexes");

            migrationBuilder.DropIndex(
                name: "IX_HousingComplexes_StreetId",
                table: "HousingComplexes");

            migrationBuilder.DropIndex(
                name: "IX_ComplexFeatures_HousingComplexId",
                table: "ComplexFeatures");

            migrationBuilder.DropIndex(
                name: "IX_Building_Location",
                table: "Buildings");

            migrationBuilder.DropIndex(
                name: "IX_Building_Street",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "HousingComplexes");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "HousingComplexes");

            migrationBuilder.DropColumn(
                name: "NeighborhoodId",
                table: "HousingComplexes");

            migrationBuilder.DropColumn(
                name: "StreetId",
                table: "HousingComplexes");

            migrationBuilder.DropColumn(
                name: "HousingComplexId",
                table: "ComplexFeatures");

            migrationBuilder.RenameIndex(
                name: "IX_PropertyImage_Property",
                table: "PropertyImages",
                newName: "IX_PropertyImages_PropertyId");

            migrationBuilder.RenameColumn(
                name: "StreetName",
                table: "Buildings",
                newName: "Street");

            migrationBuilder.AlterColumn<int>(
                name: "ManagementCompanyId",
                table: "HousingComplexes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "UsdRate",
                table: "ExchangeRates",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "EurRate",
                table: "ExchangeRates",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.CreateIndex(
                name: "IX_UserFavorites_UserId",
                table: "UserFavorites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_BuildingId",
                table: "Properties",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_UserId",
                table: "Properties",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_CityId",
                table: "Buildings",
                column: "CityId");
        }
    }
}
