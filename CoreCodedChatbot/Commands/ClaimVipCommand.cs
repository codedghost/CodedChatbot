using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Vip;
using CoreCodedChatbot.Interfaces;

using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "claimvip", "convertvip" }, false)]
    public class ClaimVipCommand : ICommand
    {
        private readonly IVipApiClient _vipApiClient;

        public ClaimVipCommand(IVipApiClient vipApiClient)
        {
            _vipApiClient = vipApiClient;
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            int.TryParse(commandText, out var numberOfTokens);
            if (numberOfTokens == 0) numberOfTokens = 1;
            var convertResponse = await _vipApiClient.ConvertBytes(new ConvertVipsRequest
            {
                Username = username,
                NumberOfBytes = numberOfTokens
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
            client.SendMessage(joinedChannel, $"Hey @{username}, this command will convert Bytes to VIPs if you have enough!");
        }
    }
}
