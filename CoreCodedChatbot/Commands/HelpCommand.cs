﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using ChatCommand = CoreCodedChatbot.CustomAttributes.ChatCommand;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "help", "commands" }, false)]
    public class HelpCommand : ICommand
    {
        public HelpCommand()
        {
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var commandsToOutput = string.Join(", ", Assembly.GetEntryAssembly().GetTypes()
                .Where(t => String.Equals(t.Namespace, "CoreCodedChatbot.Commands", StringComparison.Ordinal) &&
                            t.IsVisible)
                .Where(c => !c.GetTypeInfo().GetCustomAttribute<ChatCommand>().ModOnly)
                .Select(c =>
                    c.GetTypeInfo().GetCustomAttribute<ChatCommand>().CommandAliases[0]));

            client.SendMessage(joinedChannel, $"Supported Commands: {string.Join(", ", commandsToOutput)}");
            client.SendMessage(joinedChannel,
                "For detailed help, type !help followed by the command you want help with. Example: !help edit");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, are you sure you need help using the Help command? :D");
        }
    }
}
