using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TOEICReading.Migrations
{
    /// <inheritdoc />
    public partial class AddToeicExams : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ToeicExams",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    SourceFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToeicExams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ToeicQuestions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Part = table.Column<int>(type: "int", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Passage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptionA = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OptionB = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OptionC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OptionD = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    ToeicExamId = table.Column<long>(type: "bigint", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToeicQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToeicQuestions_ToeicExams_ToeicExamId",
                        column: x => x.ToeicExamId,
                        principalTable: "ToeicExams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToeicExams_TenantId_Code",
                table: "ToeicExams",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ToeicQuestions_TenantId_ToeicExamId_Part_Number",
                table: "ToeicQuestions",
                columns: new[] { "TenantId", "ToeicExamId", "Part", "Number" },
                unique: true,
                filter: "[TenantId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ToeicQuestions_ToeicExamId",
                table: "ToeicQuestions",
                column: "ToeicExamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToeicQuestions");

            migrationBuilder.DropTable(
                name: "ToeicExams");
        }
    }
}
