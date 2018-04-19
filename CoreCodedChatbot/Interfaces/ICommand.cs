using TwitchLib.Client;

namespace CoreCodedChatbot.Interfaces
{
    public interface ICommand
    {
        void Process(TwitchClient client, string username, string commandText, bool isMod);
        void ShowHelp(TwitchClient client, string username);
    }
}
