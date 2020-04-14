using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Vip;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{ "claimallvips", "claimall"}, false)]
    public class ClaimAllVipsCommand : ICommand
    {
        private readonly IVipApiClient _vipApiClient;

        public ClaimAllVipsCommand(IVipApiClient vipApiClient)
        {
            _vipApiClient = vipApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var convertResponse = await _vipApiClient.ConvertAllBytes(new ConvertAllVipsRequest
            {
                Username = username
            });

            if (convertResponse == null)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, Sorry I can't do that at the moment, please try again in a few minutes");
                return;
            }

            client.SendMessage(joinedChannel, convertResponse.ConvertedBytes > 0 ? $"Hey @{username}, I've converted {convertResponse.ConvertedBytes} Byte(s) to VIP(s) :D" : $"Hey @{username}, it looks like you don't have enough byte(s) to do that. Stick around and you'll have enough in no time!");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, "This command will convert all of your bytes to VIP tokens!");
        }
    }
}
