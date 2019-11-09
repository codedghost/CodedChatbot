using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Interfaces
{
    public interface IHelpHelper
    {
        void ProcessHelp(string commandName, string username, JoinedChannel joinedChannel);
    }
}