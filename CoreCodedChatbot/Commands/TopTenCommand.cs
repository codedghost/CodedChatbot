using System.Linq;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "topten", "top"}, false)]
    public class TopTenCommand : ICommand
    {
        private readonly IPlaylistApiClient _playlistApiClient;

        public TopTenCommand(IPlaylistApiClient playlistApiClient)
        {
            _playlistApiClient = playlistApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var topTen = await _playlistApiClient.GetTopTenSongs();

            if (topTen == null)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, I couldn't get that information, please try again in a few minutes");
                return;
            }

            var topTenString = string.Join(", ",
                topTen.TopTenSongs.Select((s, index) => $"{index}) {s.songRequestText}"));

            client.SendMessage(joinedChannel,
                $"Hey @{username}, here are the next 10 VIPs. {topTenString}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will tell you the next 10 VIPs in the queue! I can't mix Regular requests in here because they're played at random!");
        }
    }
}