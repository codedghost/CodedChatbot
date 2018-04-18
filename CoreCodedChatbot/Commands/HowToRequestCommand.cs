using System.Threading;
using CoreCodedChatbot.CustomAttributes;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Models.Data;
using TwitchLib;
using TwitchLib.Client;

namespace CoreCodedChatbot.Commands
{
    [ChatCommand(new[] { "howtorequest", "helprequest" }, false)]
    public class HowToRequestCommand : ICommand
    {
        private readonly ConfigModel config;

        public HowToRequestCommand(ConfigModel config)
        {
            this.config = config;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod)
        {
            client.SendMessage(config.StreamerChannel, $"To request a song just use: !request <SongArtist> - <SongTitle> - (Guitar or Bass)");
        }

        public void ShowHelp(TwitchClient client, string username)
        {
            client.SendMessage(config.StreamerChannel, $"Hey @{username}, this command outputs how to request a song from time to time.");
        }
    }
}
