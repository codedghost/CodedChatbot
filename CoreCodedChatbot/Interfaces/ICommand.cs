using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Interfaces
{
    public interface ICommand
    {
        Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel);
        void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel);
    }
}
