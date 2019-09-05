using Microsoft.EntityFrameworkCore.Migrations;

namespace CoreCodedChatbot.Database.Migrations
{
    public partial class FullGuessingGameTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SongGuessingRecord",
                columns: table => new
                {
                    SongGuessingRecordId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FinalPercentage = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsInProgress = table.Column<bool>(type: "INTEGER", nullable: false),
                    SongDetails = table.Column<string>(type: "TEXT", nullable: false),
                    UsersCanGuess = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongGuessingRecord", x => x.SongGuessingRecordId);
                });

            migrationBuilder.CreateTable(
                name: "SongPercentageGuess",
                columns: table => new
                {
                    SongPercentageGuessId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Guess = table.Column<decimal>(type: "TEXT", nullable: false),
                    SongGuessingRecordId = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongPercentageGuess", x => x.SongPercentageGuessId);
                    table.ForeignKey(
                        name: "FK_SongPercentageGuess_SongGuessingRecord_SongGuessingRecordId",
                        column: x => x.SongGuessingRecordId,
                        principalTable: "SongGuessingRecord",
                        principalColumn: "SongGuessingRecordId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SongPercentageGuess_SongGuessingRecordId",
                table: "SongPercentageGuess",
                column: "SongGuessingRecordId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SongPercentageGuess");

            migrationBuilder.DropTable(
                name: "SongGuessingRecord");
        }
    }
}
