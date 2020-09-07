using System;
using System.Linq;
using System.Reflection;
using CodedChatbot.TwitchFactories.Interfaces;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;
using ChatCommand = CoreCodedChatbot.CustomAttributes.ChatCommand;

namespace CoreCodedChatbot.Helpers
{
    public class HelpHelper : IHelpHelper
    {
        private readonly ITwitchClientFactory _twithClientFactory;

        public HelpHelper(ITwitchClientFactory twithClientFactory)
        {
            _twithClientFactory = twithClientFactory;
        }

        public void ProcessHelp(string commandName, string username, JoinedChannel joinedChannel)
        {
            var types = Assembly.GetEntryAssembly().GetTypes()
                .Where(t => String.Equals(t.Namespace, "CoreCodedChatbot.Commands", StringComparison.Ordinal) &&
                            t.IsVisible).ToList();

            var command = (ICommand) types.SingleOrDefault(c =>
                c.GetTypeInfo().GetCustomAttributes<ChatCommand>()
                    .Any(m => m.CommandAliases.Contains(commandName)));

            var twitchClient = _twithClientFactory.Get();

            if (command == null)
            {
                twitchClient.SendMessage(joinedChannel, "Sorry, I can't help with that :(");
                return;
            }

            command.ShowHelp((TwitchClient)twitchClient, username, joinedChannel);
        }
    }
}
