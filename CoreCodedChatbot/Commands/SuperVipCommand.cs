using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.Enums.Playlist;
using CoreCodedChatbot.ApiContract.RequestModels.Playlist;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{"supervip", "svip", "super"}, false)]
    public class SuperVipCommand : ICommand
    {
        private readonly IConfigService _configService;
        private readonly IPlaylistApiClient _playlistApiClient;

        public SuperVipCommand(IConfigService configService, IPlaylistApiClient playlistApiClient)
        {
            _configService = configService;
            _playlistApiClient = playlistApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod,
            JoinedChannel joinedChannel)
        {
            // Check there is a request given
            if (string.IsNullOrWhiteSpace(commandText))
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, I'm sorry but it looks like you haven't included a request! !svip <artistname> - <songname> - (guitar or bass)");
                return;
            }

            var addSuperResponse = await _playlistApiClient.AddSuperVip(new AddSuperVipRequest
            {
                Username = username,
                CommandText = commandText
            });

            if (addSuperResponse != null)
            {
                switch (addSuperResponse.Result)
                {
                    case AddRequestResult.PlaylistVeryClosed:
                        client.SendMessage(joinedChannel,
                            $"Hey @{username}, the playlist is currently very closed. No Requests allowed.");
                        return;
                    case AddRequestResult.OnlyOneSuper:
                        client.SendMessage(joinedChannel,
                            $"Hey @{username}, sorry but there can only be one SuperVIP in the queue at a time!");
                        return;
                    case AddRequestResult.NotEnoughVips:
                        client.SendMessage(joinedChannel,
                            $"Hey @{username}, it looks like you don't have enough VIP tokens :( You need at least {_configService.Get<string>("SuperVipCost")} tokens to request a SuperVIP");
                        return;
                    case AddRequestResult.Success:
                        client.SendMessage(joinedChannel,
                            $"Hey @{username}, I have queued {commandText} for you, your request will be played next!");
                        return;
                    case AddRequestResult.UnSuccessful:
                        client.SendMessage(joinedChannel,
                            $"Hey @{username}, something went wrong. Please try again in a minute");
                        return;
                }

                
            }
            else
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, something went wrong. Please try again in a minute");
            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command adds a SuperVIP to the queue for you! This request lets you completely jump the queue, but it comes with a high price!");
        }
    }
}
