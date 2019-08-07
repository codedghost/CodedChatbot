using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace CoreCodedChatbot.Database.Migrations
{
    public partial class AddingVipRequestTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "VipRequestTime",
                table: "SongRequests",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VipRequestTime",
                table: "SongRequests");
        }
    }
}
