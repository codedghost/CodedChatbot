using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{ "songlist", "playlist", "requests", "list", "songs", "sl" }, false)]
    public class SongListCommand: ICommand
    {
        private readonly IConfigService _configService;

        public SongListCommand(IConfigService configService)
        {
            _configService = configService;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                username == "Chatbot"
                    ? $"The full playlist can be found at: {_configService.Get<string>("WebPlaylistUrl")}/Chatbot/List You can now request/edit/remove requests over there too!"
                    : $"Hey @{username}, the full playlist can be found at: {_configService.Get<string>("WebPlaylistUrl")}/Chatbot/List You can now request/edit/remove requests over there too!");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command outputs a link to the current playlist");
        }
    }
}
