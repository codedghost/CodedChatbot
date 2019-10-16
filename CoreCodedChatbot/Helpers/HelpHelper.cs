using System;
using System.Linq;
using System.Reflection;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;
using ChatCommand = CoreCodedChatbot.CustomAttributes.ChatCommand;

namespace CoreCodedChatbot.Helpers
{
    public class HelpHelper : IHelpHelper
    {
        private readonly ITwitchClient client;

        public HelpHelper(ITwitchClient client)
        {
            this.client = client;
        }

        public void ProcessHelp(string commandName, string username, JoinedChannel joinedChannel)
        {
            var types = Assembly.GetEntryAssembly().GetTypes()
                .Where(t => String.Equals(t.Namespace, "CoreCodedChatbot.Commands", StringComparison.Ordinal) &&
                            t.IsVisible).ToList();

            var command = (ICommand) types.SingleOrDefault(c =>
                c.GetTypeInfo().GetCustomAttributes<ChatCommand>()
                    .Any(m => m.CommandAliases.Contains(commandName)));

            if (command == null)
            {
                client.SendMessage(joinedChannel, "Sorry, I can't help with that :(");
                return;
            }

            command.ShowHelp((TwitchClient)client, username, joinedChannel);
        }
    }
}
