using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "mybytes", "mybites" }, false)]
    public class MyBytesCommand : ICommand
    {
        private readonly IBytesHelper bytesHelper;

        public MyBytesCommand(IBytesHelper bytesHelper)
        {
            this.bytesHelper = bytesHelper;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var bytes = bytesHelper.CheckBytes(username);
            client.SendMessage(joinedChannel, $"Hey @{username}, you have {bytes} Bytes!");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will tell you how many bytes you have earned by watching the stream! To convert a Byte use !claimvip");
        }
    }
}
