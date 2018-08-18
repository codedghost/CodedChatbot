using System;
using System.Linq;
using System.Reflection;

using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "help", "commands" }, false)]
    public class HelpCommand : ICommand
    {
        private readonly ConfigModel config;

        public HelpCommand(ConfigModel config)
        {
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            var commandsToOutput = string.Join(", ", Assembly.GetEntryAssembly().GetTypes()
                .Where(t => String.Equals(t.Namespace, "CoreCodedChatbot.Commands", StringComparison.Ordinal) &&
                            t.IsVisible)
                .Where(c => !c.GetTypeInfo().GetCustomAttribute<ChatCommand>().ModOnly)
                .Select(c =>
                    c.GetTypeInfo().GetCustomAttribute<ChatCommand>().CommandAliases[0]));

            client.SendMessage(config.StreamerChannel, $"Supported Commands: {string.Join(", ", commandsToOutput)}");
            client.SendMessage(config.StreamerChannel,
                "For detailed help, type !help followed by the command you want help with. Example: !help edit");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, are you sure you need help using the Help command? :D");
        }
    }
}
