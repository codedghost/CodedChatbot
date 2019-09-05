using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace CoreCodedChatbot.Database.Migrations
{
    public partial class AddingUserInChatTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TimeLastInChat",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeLastInChat",
                table: "Users");
        }
    }
}
