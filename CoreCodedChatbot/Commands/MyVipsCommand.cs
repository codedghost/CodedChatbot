using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Vip;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{ "myvips", "mvip", "myvip", "vips"}, false)]
    public class MyVipsCommand : ICommand
    {
        private readonly IVipHelper vipHelper;
        private readonly IVipApiClient _vipApiClient;

        public MyVipsCommand(IVipHelper vipHelper,
            IVipApiClient vipApiClient)
        {
            this.vipHelper = vipHelper;
            _vipApiClient = vipApiClient;
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var getVipCountTask = _vipApiClient.GetUserVipCount(new GetUserVipCountRequest
            {
                Username = username
            });

            getVipCountTask.Wait();
            var result = getVipCountTask.Result;

            if (result == null)
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, something went wrong with the chatbot. Ask @CodedGhost2 what he's playing at!");
                return;
            }

            client.SendMessage(joinedChannel,
                result.Vips == 0
                    ? $"Hey @{username}, it looks like you have {result.Vips}. :("
                    : $"Hey @{username}, it looks like you have {result.Vips} VIPs left!");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will tell you how many VIP requests you still have!");
        }
    }
}
