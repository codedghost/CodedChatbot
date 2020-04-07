using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
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
        private readonly IVipApiClient _vipApiClient;

        public MyBytesCommand(IVipApiClient vipApiClient)
        {
            _vipApiClient = vipApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var bytes = await _vipApiClient.GetUserByteCount(username);

            if (bytes == null)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, I can't check that right now, check back in a few minutes");
                return;
            }


            client.SendMessage(joinedChannel, $"Hey @{username}, you have {bytes.Bytes} Bytes!");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will tell you how many bytes you have earned by watching the stream! To convert a Byte use !claimvip");
        }
    }
}
