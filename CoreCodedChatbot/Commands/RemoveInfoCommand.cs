using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Models.ApiRequest;
using CoreCodedChatbot.Library.Models.ApiRequest.ChatInfo;
using CoreCodedChatbot.Library.Models.ApiResponse.ChatInfo;
using CoreCodedChatbot.Library.Models.ApiResponse.Playlist;
using CoreCodedChatbot.Library.Models.Data;
using Newtonsoft.Json;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new [] {"removeinfo"}, true)]
    public class RemoveInfoCommand : ICommand
    {
        private readonly ConfigModel _configModel;
        private HttpClient chatInfoClient;

        public RemoveInfoCommand(ConfigModel configModel)
        {
            _configModel = configModel;

            chatInfoClient = new HttpClient
            {
                BaseAddress = new Uri(_configModel.ChatInfoApiUrl),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", _configModel.JwtTokenString)
                }
            };
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            // Parse Input
            var splitInput = commandText.Split('"', StringSplitOptions.RemoveEmptyEntries).ToArray();
            if (splitInput.Length != 1)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, it doesn't look like you've provided everything I need. I need at least one alias :)");
            }

            var aliases = splitInput.Single().Split(new[] {" ", ",", ", "}, StringSplitOptions.RemoveEmptyEntries);

            var requestModel = new RemoveInfoRequestModel
            {
                Aliases = aliases
            };

            var response = await chatInfoClient.PostAsync("RemoveInfo", HttpClientHelper.GetJsonData(requestModel));

            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<RemoveInfoResponseModel>(await response.Content.ReadAsStringAsync());

                client.SendMessage(joinedChannel, $"Hey @{username}, {result.ActionMessage}");
            }

            client.SendMessage(joinedChannel,
                $"Hey @{username}, I couldn't do that at the moment, please try again later :)");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command allows mods to remove information command aliases from the chatbot database! !removeinfo \"newcommandalias, nca, commandalias\"");
        }
    }
}