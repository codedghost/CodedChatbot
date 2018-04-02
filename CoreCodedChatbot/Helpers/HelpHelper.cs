using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using TwitchLib;

namespace CoreCodedChatbot.Helpers
{
    static class HelpHelper
    {
        public static void ProcessHelp(TwitchClient client, string commandName, string username)
        {
            var types = Assembly.GetEntryAssembly().GetTypes()
                .Where(t => String.Equals(t.Namespace, "CoreCodedChatbot.Commands", StringComparison.Ordinal) &&
                            t.IsVisible).ToList();

            var command = (ICommand) types.SingleOrDefault(c =>
                c.GetTypeInfo().GetCustomAttributes<ChatCommand>()
                    .Any(m => m.CommandAliases.Contains(commandName)));

            if (command == null)
            {
                client.SendMessage("Sorry, I can't help with that :(");
            }

            command.ShowHelp(client, username);
        }
    }
}
