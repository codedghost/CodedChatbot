using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Models.Data;
using Unity;
using TwitchLib.Client;

namespace CoreCodedChatbot.Helpers
{
    public class CommandHelper
    {
        private readonly IUnityContainer container;

        private List<ICommand> Commands { get; set; }
        private bool allowModCommand = true;
        private System.Threading.Timer ModCommandTimeout { get; set; }

        private readonly ConfigModel config;

        public CommandHelper(IUnityContainer container, ConfigModel config)
        {
            this.container = container;
            this.config = config;
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
                client.SendMessage(config.StreamerChannel, $"Hey @{username}, no links in the chatbot, just request the track you want!");
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
                client.SendMessage(config.StreamerChannel, $"@{username} Sorry, that command's reserved for mods only!");
                return;
            }

            if (userIsModOrBroadcaster && allowModCommand && isCommandModOnly)
            {
                TimeoutModCommand();
                command.Process(client, username, userParameters, userIsModOrBroadcaster);
            }
            else if (!isCommandModOnly)
            {
                if (command.GetType() == typeof(RockRequestCommand) || command.GetType() == typeof(VipCommand))
                {
                    TimeoutModCommand();
                }
                command.Process(client, username, userParameters, userIsModOrBroadcaster);
            }
        }

        private void TimeoutModCommand()
        {
            allowModCommand = false;
            // In seven seconds it will release the lock
            ModCommandTimeout = new System.Threading.Timer(e =>
                {
                    allowModCommand = true;
                },
                null,
                TimeSpan.FromSeconds(7),
                TimeSpan.FromSeconds(0));
        }

        private void ProcessHelp(TwitchClient client, string commandName, string username)
        {
            var command = Commands.SingleOrDefault(c =>
                c.GetType().GetTypeInfo().GetCustomAttributes<ChatCommand>()
                    .Any(m => m.CommandAliases.Contains(commandName)));

            if (command == null)
            {
                client.SendMessage(config.StreamerChannel, "Sorry, I can't help with that :(");
                return;
            }

            command.ShowHelp(client, username);
        }
    }
}
