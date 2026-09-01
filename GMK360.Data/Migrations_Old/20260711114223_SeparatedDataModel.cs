using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeparatedDataModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complexes_Neighborhoods_NeighborhoodId",
                table: "Complexes");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_AddressLocations_AddressLocationId",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Cities_CityId",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Complexes_ComplexId",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Districts_DistrictId",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Neighborhoods_NeighborhoodId",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Streets_StreetId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_CityId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_DistrictId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_NeighborhoodId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Properties_StreetId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "StreetId",
                table: "Properties");

            migrationBuilder.RenameColumn(
                name: "NeighborhoodId",
                table: "Properties",
                newName: "FloorNumber");

            migrationBuilder.RenameColumn(
                name: "DoorNumber",
                table: "Properties",
                newName: "UnitNumber");

            migrationBuilder.RenameColumn(
                name: "BuildingName",
                table: "Properties",
                newName: "TimesharePeriod");

            migrationBuilder.AlterColumn<int>(
                name: "ComplexId",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AddressLocationId",
                table: "Properties",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsTimeshare",
                table: "Properties",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeshareEndDate",
                table: "Properties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeshareStartDate",
                table: "Properties",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "Complexes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "Complexes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasPool",
                table: "Complexes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasSecurity",
                table: "Complexes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Complexes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Complexes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Complexes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "TotalUnits",
                table: "Complexes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Complexes_CityId",
                table: "Complexes",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Complexes_DistrictId",
                table: "Complexes",
                column: "DistrictId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complexes_Cities_CityId",
                table: "Complexes",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Complexes_Districts_DistrictId",
                table: "Complexes",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Complexes_Neighborhoods_NeighborhoodId",
                table: "Complexes",
                column: "NeighborhoodId",
                principalTable: "Neighborhoods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_AddressLocations_AddressLocationId",
                table: "Properties",
                column: "AddressLocationId",
                principalTable: "AddressLocations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Complexes_ComplexId",
                table: "Properties",
                column: "ComplexId",
                principalTable: "Complexes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Complexes_Cities_CityId",
                table: "Complexes");

            migrationBuilder.DropForeignKey(
                name: "FK_Complexes_Districts_DistrictId",
                table: "Complexes");

            migrationBuilder.DropForeignKey(
                name: "FK_Complexes_Neighborhoods_NeighborhoodId",
                table: "Complexes");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_AddressLocations_AddressLocationId",
                table: "Properties");

            migrationBuilder.DropForeignKey(
                name: "FK_Properties_Complexes_ComplexId",
                table: "Properties");

            migrationBuilder.DropIndex(
                name: "IX_Complexes_CityId",
                table: "Complexes");

            migrationBuilder.DropIndex(
                name: "IX_Complexes_DistrictId",
                table: "Complexes");

            migrationBuilder.DropColumn(
                name: "IsTimeshare",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "TimeshareEndDate",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "TimeshareStartDate",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Complexes");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Complexes");

            migrationBuilder.DropColumn(
                name: "HasPool",
                table: "Complexes");

            migrationBuilder.DropColumn(
                name: "HasSecurity",
                table: "Complexes");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Complexes");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Complexes");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Complexes");

            migrationBuilder.DropColumn(
                name: "TotalUnits",
                table: "Complexes");

            migrationBuilder.RenameColumn(
                name: "UnitNumber",
                table: "Properties",
                newName: "DoorNumber");

            migrationBuilder.RenameColumn(
                name: "TimesharePeriod",
                table: "Properties",
                newName: "BuildingName");

            migrationBuilder.RenameColumn(
                name: "FloorNumber",
                table: "Properties",
                newName: "NeighborhoodId");

            migrationBuilder.AlterColumn<int>(
                name: "ComplexId",
                table: "Properties",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AddressLocationId",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Properties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Properties",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StreetId",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Properties_CityId",
                table: "Properties",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_DistrictId",
                table: "Properties",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_NeighborhoodId",
                table: "Properties",
                column: "NeighborhoodId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_StreetId",
                table: "Properties",
                column: "StreetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Complexes_Neighborhoods_NeighborhoodId",
                table: "Complexes",
                column: "NeighborhoodId",
                principalTable: "Neighborhoods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_AddressLocations_AddressLocationId",
                table: "Properties",
                column: "AddressLocationId",
                principalTable: "AddressLocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Cities_CityId",
                table: "Properties",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Complexes_ComplexId",
                table: "Properties",
                column: "ComplexId",
                principalTable: "Complexes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Districts_DistrictId",
                table: "Properties",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Neighborhoods_NeighborhoodId",
                table: "Properties",
                column: "NeighborhoodId",
                principalTable: "Neighborhoods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Properties_Streets_StreetId",
                table: "Properties",
                column: "StreetId",
                principalTable: "Streets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
