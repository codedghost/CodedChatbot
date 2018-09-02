using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace CoreCodedChatbot.Database.Migrations
{
    public partial class SongGuessingGameTables : Migration
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
                    Username = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongPercentageGuess", x => x.SongPercentageGuessId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SongGuessingRecord");

            migrationBuilder.DropTable(
                name: "SongPercentageGuess");
        }
    }
}
