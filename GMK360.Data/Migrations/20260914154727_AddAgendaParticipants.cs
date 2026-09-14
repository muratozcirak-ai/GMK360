using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAgendaParticipants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgendaParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgendaRecordId = table.Column<int>(type: "int", nullable: false),
                    PhonebookId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParticipantNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsViewed = table.Column<bool>(type: "bit", nullable: false),
                    ReminderSent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendaParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgendaParticipants_AgencyPhonebooks_PhonebookId",
                        column: x => x.PhonebookId,
                        principalTable: "AgencyPhonebooks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgendaParticipants_AgendaRecords_AgendaRecordId",
                        column: x => x.AgendaRecordId,
                        principalTable: "AgendaRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgendaParticipants_AgendaRecordId",
                table: "AgendaParticipants",
                column: "AgendaRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaParticipants_PhonebookId",
                table: "AgendaParticipants",
                column: "PhonebookId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgendaParticipants");
        }
    }
}
