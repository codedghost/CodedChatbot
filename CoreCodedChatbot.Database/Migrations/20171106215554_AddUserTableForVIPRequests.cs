using Microsoft.EntityFrameworkCore.Migrations;

namespace CoreCodedChatbot.Database.Migrations
{
    public partial class AddUserTableForVIPRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    DonationOrBitsVipRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    FollowVipRequest = table.Column<int>(type: "INTEGER", nullable: false),
                    ModGivenVipRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    SubVipRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    UsedVipRequests = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Username);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
