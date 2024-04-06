using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.ApiClients;
using CoreCodedChatbot.Commands;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using ChatCommand = CoreCodedChatbot.CustomAttributes.ChatCommand;

namespace CoreCodedChatbot.Helpers
{
    public class CommandHelper : ICommandHelper
    {

        private List<ICommand> Commands { get; set; }
        private bool allowModCommand = true;
        private System.Threading.Timer ModCommandTimeout { get; set; }
        private readonly ICustomChatCommandsClient _customChatCommandsClient;

        public CommandHelper(
            ICustomChatCommandsClient customChatCommandsClient
            )
        {
            _customChatCommandsClient = customChatCommandsClient;
        }

        public void Init(IServiceProvider serviceProvider)
        {
            Commands = new List<ICommand>();

            var types = Assembly.GetEntryAssembly().GetTypes()
                .Where(t => string.Equals(t.Namespace, "CoreCodedChatbot.Commands", StringComparison.Ordinal) && t.IsVisible).ToList();

            foreach (var type in types)
            {
                var service = serviceProvider.GetService(type);
                Commands.Add((ICommand) service);
            }
        }

        public async Task ProcessCommand(string userCommand, TwitchClient client, string username,
            string userParameters, bool userIsModOrBroadcaster, JoinedChannel joinedRoom)
        {
            if ((userParameters.Contains("www.", StringComparison.InvariantCultureIgnoreCase) || 
                 userParameters.Contains("http", StringComparison.InvariantCultureIgnoreCase)) && 
                !userCommand.Contains("info", StringComparison.InvariantCultureIgnoreCase))
            {
                client.SendMessage(joinedRoom, $"Hey @{username}, no links in the chatbot, just request the track you want!");
                return;
            }

            if (userCommand.ToLower() == "help" && !string.IsNullOrWhiteSpace(userParameters))
            {
                await ProcessHelp(client, userParameters.ToLower(), username, joinedRoom);
                return;
            }
            
            var command = Commands.SingleOrDefault(c =>
                c.GetType().GetTypeInfo().GetCustomAttributes<ChatCommand>()
                    .Any(m => m.CommandAliases.ToLower() == userCommand.ToLower()));

            if (command == null)
            {
                // Check if this command exists in the db as an info command
                var infoText = await _customChatCommandsClient.GetCommandText(userCommand.ToLower());

                if (infoText != null)
                {
                    command = Commands.Single(c => c.GetType().GetTypeInfo().GetCustomAttributes<ChatCommand>()
                        .Any(m => m.CommandAliases.Contains("chatbotinfocommand")));

                    await command.Process(client, userParameters, infoText.CommandText, userIsModOrBroadcaster,
                        joinedRoom);

                    return;
                }
            }

            if (command == null) return;

            var isCommandModOnly = command.GetType().GetTypeInfo().GetCustomAttributes<ChatCommand>().Any(m => m.ModOnly);

            if (!userIsModOrBroadcaster && isCommandModOnly)
            {
                client.SendMessage(joinedRoom, $"@{username} Sorry, that command's reserved for mods only!");
                return;
            }

            if (userIsModOrBroadcaster && allowModCommand && isCommandModOnly)
            {
                TimeoutModCommand();
                await command.Process(client, username, userParameters, userIsModOrBroadcaster, joinedRoom);
            }
            else if (!isCommandModOnly)
            {
                if (command.GetType() == typeof(RockRequestCommand) || command.GetType() == typeof(VipCommand))
                {
                    TimeoutModCommand();
                }
                await command.Process(client, username, userParameters, userIsModOrBroadcaster, joinedRoom);
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

        private async Task ProcessHelp(TwitchClient client, string commandName, string username, JoinedChannel joinedChannel)
        {
            var command = Commands.SingleOrDefault(c =>
                c.GetType().GetTypeInfo().GetCustomAttributes<ChatCommand>()
                    .Any(m => m.CommandAliases.Contains(commandName)));

            if (command == null)
            {
                var helpText = await _customChatCommandsClient.GetCommandHelpText(commandName);
                if (helpText != null)
                {
                    client.SendMessage(joinedChannel, string.Format(helpText.HelpText, username));
                    return;
                }

                client.SendMessage(joinedChannel, "Sorry, I can't help with that :(");
                return;
            }

            command.ShowHelp(client, username, joinedChannel);
        }
    }
}
