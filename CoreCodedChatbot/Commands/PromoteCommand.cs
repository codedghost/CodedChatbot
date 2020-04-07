using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.Enums.Playlist;
using CoreCodedChatbot.ApiContract.RequestModels.Playlist;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new [] { "promote"}, false)]
    public class PromoteCommand : ICommand
    {
        private readonly IPlaylistApiClient _playlistApiClient;

        public PromoteCommand(IPlaylistApiClient playlistApiClient)
        {
            _playlistApiClient = playlistApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod,
            JoinedChannel joinedChannel)
        {
            var promoteSongResponse = await _playlistApiClient.PromoteSong(new PromoteSongRequest
            {
                Username = username
            });

            switch (promoteSongResponse.PromoteRequestResult)
            {
                case PromoteRequestResult.NotYourRequest:
                    client.SendMessage(joinedChannel,
                        $"Hey @{username}, I'm sorry but that request doesn't seem to belong to you. Please check your requests with !myrequests");
                    return;
                case PromoteRequestResult.UnSuccessful:
                    client.SendMessage(joinedChannel,
                        $"Hey @{username}, sorry I can't promote your request right now, please try again in a sec");
                    return;
                case PromoteRequestResult.NoVipAvailable:
                    client.SendMessage(joinedChannel,
                        $"Hey @{username}, sorry but you don't have a VIP to promote this request");
                    return;
                case PromoteRequestResult.Successful:
                    client.SendMessage(joinedChannel,
                        $"Hey @{username}, I have promoted your request to #{promoteSongResponse.PlaylistIndex} for you!");
                    return;
            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, if you have a VIP token, this command will bump your song request right to the top of the queue. Usage: !promote");
        }
    }
}
