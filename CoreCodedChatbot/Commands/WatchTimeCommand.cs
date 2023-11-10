using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.DataHelper;
using CoreCodedChatbot.ApiContract.ResponseModels.WatchTime;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Secrets;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] {"watchtime"}, false)]
    public class WatchTimeCommand : ICommand
    {
        private readonly ILogger<WatchTimeCommand> _logger;
        private readonly HttpClient _watchTimeApiClient;

        public WatchTimeCommand(
            IConfigService configService,
            ISecretService secretService,
            ILogger<WatchTimeCommand> logger)
        {
            _logger = logger;
            _watchTimeApiClient = HttpClientHelper.BuildClient(configService, secretService, "WatchTime");
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var watchTimeResponse = await _watchTimeApiClient.GetAsync<GetWatchTimeResponse>($"GetWatchTime?username={username}", _logger);

            if (watchTimeResponse == null || watchTimeResponse.WatchTime == TimeSpan.Zero)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, I had some trouble getting your Watch Time, please try again in a few minutes");
                return;
            }

            var formattedWatchTime = GetWatchTimeString(watchTimeResponse.WatchTime);

            client.SendMessage(joinedChannel,
                $"Hey @{username}, You have watched the channel for a total of {formattedWatchTime}");
        }

        public string GetWatchTimeString(TimeSpan watchTime)
        {
            var returnString = new StringBuilder();
            if (watchTime.Days > 0)
            {
                returnString.Append($"{watchTime.Days} Days, ");
            }

            if (watchTime.Hours > 0)
            {
                returnString.Append($"{watchTime.Hours} Hours ");
            }

            if (!string.IsNullOrWhiteSpace(returnString.ToString()))
            {
                returnString.Append("and ");
            }

            returnString.Append($"{watchTime.Minutes} Minutes");

            return returnString.ToString();
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will let you know how long you've watched the channel!");
        }
    }
}