using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GMK360.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDnaTemplateEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectDnaTemplateOptions");

            migrationBuilder.CreateTable(
                name: "DnaTemplateSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StepName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DnaTemplateSteps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DnaTemplateQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DnaTemplateStepId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsMultipleChoice = table.Column<bool>(type: "bit", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    DefaultWorkGroupDefinitionId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    QuestionOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DnaTemplateQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DnaTemplateQuestions_DnaTemplateSteps_DnaTemplateStepId",
                        column: x => x.DnaTemplateStepId,
                        principalTable: "DnaTemplateSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DnaTemplateQuestions_WorkGroupDefinitions_DefaultWorkGroupDefinitionId",
                        column: x => x.DefaultWorkGroupDefinitionId,
                        principalTable: "WorkGroupDefinitions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DnaTemplateOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DnaTemplateQuestionId = table.Column<int>(type: "int", nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    WorkGroupDefinitionId = table.Column<int>(type: "int", nullable: true),
                    OptionOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DnaTemplateOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DnaTemplateOptions_DnaTemplateQuestions_DnaTemplateQuestionId",
                        column: x => x.DnaTemplateQuestionId,
                        principalTable: "DnaTemplateQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DnaTemplateOptions_WorkGroupDefinitions_WorkGroupDefinitionId",
                        column: x => x.WorkGroupDefinitionId,
                        principalTable: "WorkGroupDefinitions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DnaTemplateOptions_DnaTemplateQuestionId",
                table: "DnaTemplateOptions",
                column: "DnaTemplateQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_DnaTemplateOptions_WorkGroupDefinitionId",
                table: "DnaTemplateOptions",
                column: "WorkGroupDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_DnaTemplateQuestions_DefaultWorkGroupDefinitionId",
                table: "DnaTemplateQuestions",
                column: "DefaultWorkGroupDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_DnaTemplateQuestions_DnaTemplateStepId",
                table: "DnaTemplateQuestions",
                column: "DnaTemplateStepId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DnaTemplateOptions");

            migrationBuilder.DropTable(
                name: "DnaTemplateQuestions");

            migrationBuilder.DropTable(
                name: "DnaTemplateSteps");

            migrationBuilder.CreateTable(
                name: "ProjectDnaTemplateOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkGroupDefinitionId = table.Column<int>(type: "int", nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDnaTemplateOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectDnaTemplateOptions_WorkGroupDefinitions_WorkGroupDefinitionId",
                        column: x => x.WorkGroupDefinitionId,
                        principalTable: "WorkGroupDefinitions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDnaTemplateOptions_WorkGroupDefinitionId",
                table: "ProjectDnaTemplateOptions",
                column: "WorkGroupDefinitionId");
        }
    }
}
