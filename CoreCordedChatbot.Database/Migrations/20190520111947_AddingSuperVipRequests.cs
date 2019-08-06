using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CoreCodedChatbot.Database.Migrations
{
    public partial class AddingSuperVipRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsedSuperVipRequests",
                table: "Users",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuperVipRequestTime",
                table: "SongRequests",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsedSuperVipRequests",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SuperVipRequestTime",
                table: "SongRequests");
        }
    }
}
