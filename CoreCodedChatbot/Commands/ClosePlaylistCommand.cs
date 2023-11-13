using System;
using System.Net.Http;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.DataHelper;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Secrets;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "cp", "closeplaylist", "sp", "shutplaylist" }, true)]
    public class ClosePlaylistCommand : ICommand
    {
        private readonly ILogger<ClosePlaylistCommand> _logger;
        private readonly HttpClient _playlistApiClient;

        public ClosePlaylistCommand(
            IConfigService configService,
            ISecretService secretService,
            ILogger<ClosePlaylistCommand> logger)
        {
            _logger = logger;
            _playlistApiClient = HttpClientHelper.BuildClient(configService, secretService, "Playlist");
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod,
            JoinedChannel joinedChannel)
        {
            var closePlaylistUrl = "ClosePlaylist";
            if (commandText.Equals("very", StringComparison.OrdinalIgnoreCase))
            {
                closePlaylistUrl = "VeryClosePlaylist";
            }

            bool response;
            try
            {
                var apiResponse = await _playlistApiClient.GetAsync(closePlaylistUrl);

                response = apiResponse.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                response = HttpClientHelper.LogError<bool>(_logger, e, new object[] { });
            }

            client.SendMessage(joinedChannel, response
                ? $"Hey @{username}, I have closed the playlist{(commandText.Equals("very", StringComparison.OrdinalIgnoreCase) ? " completely" : string.Empty)}!"
                : $"Hey {username}, I can't seem to close the playlist for some reason :(");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will close the playlist!");
        }
    }
}
