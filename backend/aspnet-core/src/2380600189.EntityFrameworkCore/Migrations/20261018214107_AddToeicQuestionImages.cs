using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TOEICReading.Migrations
{
    /// <inheritdoc />
    public partial class AddToeicQuestionImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ToeicQuestionImages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Data = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    ToeicQuestionId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToeicQuestionImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToeicQuestionImages_ToeicQuestions_ToeicQuestionId",
                        column: x => x.ToeicQuestionId,
                        principalTable: "ToeicQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToeicQuestionImages_ToeicQuestionId_SortOrder",
                table: "ToeicQuestionImages",
                columns: new[] { "ToeicQuestionId", "SortOrder" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToeicQuestionImages");
        }
    }
}
