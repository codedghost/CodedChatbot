using Microsoft.EntityFrameworkCore.Migrations;

namespace CoreCodedChatbot.Database.Migrations
{
    public partial class AddingCustomsForgeInfoCommand : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("InfoCommands",new []
            {
                "InfoCommandId",
                "InfoText",
                "InfoHelpText"
            }, new object[,]
            {
                {1, "{0}Create a login and find songs to choose from at: http://ignition.customsforge.com", "Hey @{0}, this command outputs where to find CDLC from time to time." }
            });

            migrationBuilder.InsertData("InfoCommandKeywords", new[] { "InfoCommandKeywordText", "InfoCommandId" }, new object[,] { { "customsforge", 1 }, { "cf", 1 }, { "customforge", 1 } });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("InfoCommand", "InfoCommandId", 1);
            migrationBuilder.DeleteData("InfoCommandKeywords", "InfoCommandId", 1);
        }
    }
}
