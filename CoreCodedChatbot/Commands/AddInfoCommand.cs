using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Database.Context.Models;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "addinfo" }, true)]
    public class AddInfoCommand : ICommand
    {
        private ChatbotContextFactory chatbotContextFactory;

        public AddInfoCommand(ChatbotContextFactory chatbotContextFactory)
        {
            this.chatbotContextFactory = chatbotContextFactory;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            try
            {
                // Parse Input
                var splitInput = commandText.Split('"', StringSplitOptions.RemoveEmptyEntries).ToArray();
                if (splitInput.Length != 3)
                {
                    client.SendMessage(joinedChannel,
                        $"Hey @{username}, it doesn't look like you've provided everything I need. I need aliases, text, and helptext :)");
                    return;
                }

                var aliases = splitInput[0]
                    .Split(new[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries)
                    .ToArray();
                var info = splitInput[1];
                var helpText = splitInput[2];

                if (aliases.Length == 0
                    || string.IsNullOrWhiteSpace(info)
                    || string.IsNullOrWhiteSpace(helpText))
                {
                    client.SendMessage(joinedChannel,
                        $"Hey @{username}, it doesn't look like you've provided everything I need. I need aliases, text, and helptext :)");
                    return;
                }

                // Check that info aliases aren't already in use
                using (var context = chatbotContextFactory.Create())
                {
                    if (context.InfoCommandKeywords.Any(ik => aliases.Contains(ik.InfoCommandKeywordText)))
                    {
                        client.SendMessage(joinedChannel,
                            $"Hey @{username}, looks like one of those commands is already in use! Please check your aliases and try again");
                        return;
                    }

                    // Add command to db
                    var infoCommand = new Database.Context.Models.InfoCommand
                    {
                        InfoText = "{0}" + info,
                        InfoHelpText = "Hey {0}! " + helpText
                    };

                    context.InfoCommands.Add(infoCommand);

                    // Add aliases
                    var infoCommandKeywords = aliases.Select(a => new InfoCommandKeyword
                    {
                        InfoCommandId = infoCommand.InfoCommandId,
                        InfoCommandKeywordText = a
                    });

                    context.InfoCommandKeywords.AddRange(infoCommandKeywords);
                    context.SaveChanges();
                }

                // Respond
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, I have added that command, give it a quick go by using any of these commands: {string.Join(", ", aliases)}");
            }
            catch (Exception e)
            {
                Console.Out.WriteLine($"{e} + {e.InnerException}");
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, sorry but I couldn't manage to add that command at the moment, please try again later");
            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey {username}, this command allows mods to add information commands to the chatbot database! !addinfo \"newcommandalias, nca, commandalias\" \"This is the new info command text\" \"This is a piece of help text for when a user types !help newcommandalias\" ");
        }
    }
}
