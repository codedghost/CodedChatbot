using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Vip;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{ "myvips", "mvip", "myvip", "vips"}, false)]
    public class MyVipsCommand : ICommand
    {
        private readonly IVipApiClient _vipApiClient;

        public MyVipsCommand(IVipApiClient vipApiClient)
        {
            _vipApiClient = vipApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var getVipCountTask = await _vipApiClient.GetUserVipCount(new GetUserVipCountRequest
            {
                Username = username
            });

            if (getVipCountTask == null)
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, something went wrong with the chatbot. Ask @CodedGhost2 what he's playing at!");
                return;
            }

            client.SendMessage(joinedChannel,
                getVipCountTask.Vips == 0
                    ? $"Hey @{username}, it looks like you have {getVipCountTask.Vips}. :("
                    : $"Hey @{username}, it looks like you have {getVipCountTask.Vips} VIPs left!");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will tell you how many VIP requests you still have!");
        }
    }
}
