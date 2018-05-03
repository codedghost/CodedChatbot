using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Models.Data;

using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new []{ "songlist", "playlist", "requests", "list", "songs" }, true)]
    public class SongListCommand: ICommand
    {
        private readonly ConfigModel config;

        public SongListCommand(ConfigModel config)
        {
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, the full playlist can be found at: http://localhost:49420/playlist");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command outputs a link to the current playlist");
        }
    }
}
