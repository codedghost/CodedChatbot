using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace CoreCodedChatbot.Database.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SongRequests",
                columns: table => new
                {
                    SongRequestId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Played = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequestText = table.Column<string>(type: "TEXT", nullable: true),
                    RequestTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RequestUsername = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    SongId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SongRequests", x => x.SongRequestId);
                });

            migrationBuilder.CreateTable(
                name: "Songs",
                columns: table => new
                {
                    SongId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SongArtist = table.Column<string>(type: "TEXT", nullable: false),
                    SongName = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Songs", x => x.SongId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SongRequests");

            migrationBuilder.DropTable(
                name: "Songs");
        }
    }
}
