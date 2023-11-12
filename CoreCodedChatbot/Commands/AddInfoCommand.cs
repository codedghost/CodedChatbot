using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.ApiClients;
using CoreCodedChatbot.ApiClient.DataHelper;
using CoreCodedChatbot.ApiContract.RequestModels.CustomChatCommands;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Secrets;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Interfaces;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "addinfo" }, true)]
    public class AddInfoCommand : ICommand
    {
        private readonly HttpClient _customChatCommandsClient;
        private readonly ILogger<AddInfoCommand> _logger;

        public AddInfoCommand(
            IConfigService configService,
            ISecretService secretService,
            ILogger<AddInfoCommand> logger
            )
        {
            _customChatCommandsClient = HttpClientHelper.BuildClient(configService, secretService, "CustomChatCommands");
            _logger = logger;
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            try
            {
                // Parse Input
                var splitInput = commandText.Split('"', StringSplitOptions.RemoveEmptyEntries)
                    .Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
                if (splitInput.Length != 3)
                {
                    client.SendMessage(joinedChannel,
                        $"Hey @{username}, it doesn't look like you've provided everything I need. I need aliases, text, and helptext :)");
                    return;
                }

                var aliases = splitInput[0]
                    .Split(new[] { ", ", "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(k => !string.IsNullOrWhiteSpace(k))
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

                bool success;
                var request = new AddCommandRequest
                {
                    Aliases = aliases.ToList(),
                    InformationText = info,
                    HelpText = helpText,
                    Username = username
                };

                try
                {
                    var result =
                        await _customChatCommandsClient.PostAsync("AddCommand", HttpClientHelper.GetJsonData(request));

                    success = result.IsSuccessStatusCode;
                }
                catch (Exception e)
                {
                    success = HttpClientHelper.LogError<bool>(_logger, e,
                        new object[] {request.Aliases, request.InformationText, request.HelpText, request.Username});
                }

                // Respond
                client.SendMessage(joinedChannel,
                    success
                        ? $"Hey @{username}, I have added that command, give it a quick go by using any of these commands: {string.Join(", ", aliases)}"
                        : $"Hey @{username}, I couldn't add that command right now, please try again in a few minutes or type !help addinfo");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in AddInfoCommand");
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, sorry but I couldn't manage to add that command at the moment, please try again later");
            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command allows mods to add information commands to the chatbot database! !addinfo \"newcommandalias, nca, commandalias\" \"This is the new info command text\" \"This is a piece of help text for when a user types !help newcommandalias\" ");
        }
    }
}
