using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers.Interfaces;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Models.Data;

using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new []{ "songlist", "playlist", "requests", "list", "songs" }, false)]
    public class SongListCommand: ICommand
    {
        private readonly ConfigModel config;

        public SongListCommand(IConfigHelper configHelper)
        {
            this.config = configHelper.GetConfig();
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {

            client.SendMessage(config.StreamerChannel, $"Hey @{username}, the full playlist can be found at: {config.WebPlaylistUrl}/Chatbot/List");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command outputs a link to the current playlist");
        }
    }
}
