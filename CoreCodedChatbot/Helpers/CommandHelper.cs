using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Net.Sockets;
using System.Diagnostics;

using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Commands;
using CoreCodedChatbot.CustomAttributes;

using TwitchLib;

using Unity;

namespace CoreCodedChatbot.Helpers
{
    public class CommandHelper
    {
        private readonly IUnityContainer container;

        private List<ICommand> Commands { get; set; }
        private bool allowModCommand = true;
        private System.Threading.Timer ModCommandTimeout { get; set; }

        public CommandHelper(IUnityContainer container)
        {
            this.container = container;
        }

        public void Init()
        {
            Commands = new List<ICommand>();

            var types = Assembly.GetEntryAssembly().GetTypes()
                .Where(t => String.Equals(t.Namespace, "CoreCodedChatbot.Commands", StringComparison.Ordinal) && t.IsVisible).ToList();

            foreach (var type in types)
            {
                Commands.Add((ICommand)container.Resolve(type));
            }
        }

        public void ProcessCommand(string userCommand, TwitchClient client, string username,
            string userParameters, bool userIsModOrBroadcaster)
        {
            var command = Commands.SingleOrDefault(c =>
                c.GetType().GetTypeInfo().GetCustomAttributes<ChatCommand>()
                    .Any(m => m.CommandAliases.Contains(userCommand.ToLower())));

            if (command == null) return;

            if (userParameters.Contains("www.") || userParameters.Contains("http"))
            {
                client.SendMessage($"Hey @{username}, no links in the chatbot, just request the track you want!");
                return;
            }

            if (userCommand.ToLower() == "help" && !string.IsNullOrWhiteSpace(userParameters))
            {
                ProcessHelp(client, userParameters.ToLower(), username);
                return;
            }

            var isCommandModOnly = command.GetType().GetTypeInfo().GetCustomAttributes<ChatCommand>().Any(m => m.ModOnly);

            if (!userIsModOrBroadcaster && isCommandModOnly)
            {
                client.SendMessage($"@{username} Sorry, that command's reserved for mods only!");
                return;
            }

            if (userIsModOrBroadcaster && allowModCommand && isCommandModOnly)
            {
                allowModCommand = false;
                // In two seconds it will release the lock
                ModCommandTimeout = new System.Threading.Timer(e =>
                {
                    allowModCommand = true;
                },
                null,
                TimeSpan.FromSeconds(7),
                TimeSpan.FromSeconds(0));

                command.Process(client, username, userParameters, userIsModOrBroadcaster);
            }
            else if (!isCommandModOnly)
            {
                command.Process(client, username, userParameters, userIsModOrBroadcaster);
            }
        }
        
        public void ProcessHelp(TwitchClient client, string commandName, string username)
        {
            var command = Commands.SingleOrDefault(c =>
                c.GetType().GetTypeInfo().GetCustomAttributes<ChatCommand>()
                    .Any(m => m.CommandAliases.Contains(commandName)));

            if (command == null)
            {
                client.SendMessage("Sorry, I can't help with that :(");
            }

            command.ShowHelp(client, username);
        }
    }
}
