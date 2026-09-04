using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConstructionTaskInvites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConstructionTaskInvites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConstructionTaskId = table.Column<int>(type: "int", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    QuoteAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    QuoteNote = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InviteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResponseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentByUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MatchedServiceProviderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConstructionTaskInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConstructionTaskInvites_ConstructionTasks_ConstructionTaskId",
                        column: x => x.ConstructionTaskId,
                        principalTable: "ConstructionTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConstructionTaskInvites_ConstructionTaskId",
                table: "ConstructionTaskInvites",
                column: "ConstructionTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConstructionTaskInvites");
        }
    }
}
