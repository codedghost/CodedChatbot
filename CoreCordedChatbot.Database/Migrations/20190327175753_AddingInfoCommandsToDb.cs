using Microsoft.EntityFrameworkCore.Migrations;

namespace CoreCodedChatbot.Database.Migrations
{
    public partial class AddingInfoCommandsToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InfoCommands",
                columns: table => new
                {
                    InfoCommandId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    InfoText = table.Column<string>(nullable: false),
                    InfoHelpText = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfoCommands", x => x.InfoCommandId);
                });

            migrationBuilder.CreateTable(
                name: "InfoCommandKeywords",
                columns: table => new
                {
                    InfoCommandKeywordText = table.Column<string>(nullable: false),
                    InfoCommandId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InfoCommandKeywords", x => new { x.InfoCommandId, x.InfoCommandKeywordText });
                    table.ForeignKey(
                        name: "FK_InfoCommandKeywords_InfoCommands_InfoCommandId",
                        column: x => x.InfoCommandId,
                        principalTable: "InfoCommands",
                        principalColumn: "InfoCommandId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InfoCommandKeywords");

            migrationBuilder.DropTable(
                name: "InfoCommands");
        }
    }
}
