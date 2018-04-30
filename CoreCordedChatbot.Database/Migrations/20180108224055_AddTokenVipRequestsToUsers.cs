using Microsoft.EntityFrameworkCore.Migrations;

namespace CoreCodedChatbot.Database.Migrations
{
    public partial class AddTokenVipRequestsToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TokenVipRequests",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenVipRequests",
                table: "Users");
        }
    }
}
