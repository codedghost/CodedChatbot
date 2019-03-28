using Microsoft.EntityFrameworkCore.Migrations;

namespace CoreCodedChatbot.Database.Migrations
{
    public partial class AddingOtherInformationCommands : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("InfoCommands",
                new[]
                {
                    "InfoCommandId",
                    "InfoText",
                    "InfoHelpText"
                },
                new object[,]
                {
                    {
                        2,
                        "{0}Some streamers have their strings reversed (or inverse) in Rocksmith because some people find it much easier to read this way. It is very similar to the way that guitar tabs are read and can be a lot easier for some. Hope this helps!",
                        "Hey @{0}, this command will explain why some streamers have their strings upside down in Rocksmith!"
                    },
                    {
                        3,
                        "{0}Want to take part in our challenge where we learn a new song every month? Join the discord and react on the info page to get access to the challenge channel! ===> !discord",
                        "Hey @{0}, this command will tell you about the Rocksmith Challenge we run each month!"
                    },
                    {
                        4,
                        "{0}This is Rocksmith 2014 Remastered Edition! Check it out here: https://rocksmith.ubisoft.com/rocksmith/en-us/home/",
                        "Hey @{0}, this command will tell you about Rocksmith"
                    }
                });

            migrationBuilder.InsertData("InfoCommandKeywords",
                new[]
                {
                    "InfoCommandKeywordText",
                    "InfoCommandId"
                },
                new object[,]
                {
                    {
                        "reverse",
                        2
                    },
                    {
                        "inverse",
                        2
                    },
                    {
                        "aussie",
                        2
                    },
                    {
                        "australian",
                        2
                    },
                    {
                        "rocksmithchallenge",
                        3
                    },
                    {
                        "challenge",
                        3
                    },
                    {
                        "rocksmith",
                        4
                    },
                    {
                        "rs",
                        4
                    }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
