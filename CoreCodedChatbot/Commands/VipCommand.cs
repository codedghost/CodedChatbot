using System.Threading;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Models.Data;
using TwitchLib;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new []{"vip", "viprequest"}, false)]
    public class VipCommand : ICommand
    {
        private readonly PlaylistHelper playlistHelper;

        private readonly VipHelper vipHelper;

        private readonly ConfigModel config;

        public VipCommand(VipHelper vipHelper, PlaylistHelper playlistHelper, ConfigModel config)
        {
            this.vipHelper = vipHelper;
            this.playlistHelper = playlistHelper;
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                client.SendMessage(config.StreamerChannel, $"Hey @{username}, looks like you haven't included a request there!");
                return;
            }

            if (vipHelper.CanUseVipRequest(username))
            {
                var playlistPosition = 0;
                var songIndex = 0;
                if (int.TryParse(commandText.Trim('#'), out songIndex))
                {
                    playlistPosition = playlistHelper.PromoteRequest(username, songIndex-1);
                    client.SendMessage(config.StreamerChannel, playlistPosition == -1
                        ? $"Hey @{username}, I can't find a song at that position! Please check your requests with !myrequests"
                        : playlistPosition == -2
                            ? $"Hey @{username}, I'm sorry but that request doesn't seem to belong to you. Please check your requests with !myrequests"
                            : playlistPosition == 0
                                ? $"Hey @{username}, something seems to have gone wrong. Please try again in a minute or two"
                                : $"Hey @{username}, I have promoted #{commandText} to #{playlistPosition} for you!");

                    if (playlistPosition > 0) vipHelper.UseVipRequest(username);
                    return;
                }

                (_, playlistPosition) = playlistHelper.AddRequest(username, commandText, true);
                vipHelper.UseVipRequest(username);
                client.SendMessage(config.StreamerChannel,
                    $"Hey @{username}, I have queued {commandText} for you, you're #{playlistPosition} in the queue!");
            }
            else
            {
                client.SendMessage(config.StreamerChannel,
                    $"Hey @{username}, it looks like you don't have any remaining VIP requests. Please use the standard !request command.");
            }
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel,
                $"Hey @{username}, if you have a VIP request, this command will bump your song request right to the top of the queue. Usage: !vip <SongArtist> - <SongName> - (Guitar or Bass) OR !vip <SongNumber>");
        }
    }
}
