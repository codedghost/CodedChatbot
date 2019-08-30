using Microsoft.EntityFrameworkCore.Migrations;

namespace CoreCodedChatbot.Database.Migrations
{
    public partial class AddingGiftedVips : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReceivedGiftVipRequests",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddColumn<int>(
                name: "SendgiftVipRequests",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceivedGiftVipRequests",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SendgiftVipRequests",
                table: "Users");
        }
    }
}
