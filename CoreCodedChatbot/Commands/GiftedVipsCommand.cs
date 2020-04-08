using System.Linq;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new []{"giftedvips", "gifted"}, false)]
    public class GiftedVipsCommand : ICommand
    {
        private readonly IVipApiClient _vipApiClient;

        public GiftedVipsCommand(IVipApiClient vipApiClient)
        {
            _vipApiClient = vipApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var usernameToCheck = username;
            if (!string.IsNullOrWhiteSpace(commandText) && commandText.Contains("@"))
            {
                username = commandText.Split(" ").First();
            }

            var giftedVips = await _vipApiClient.GetGiftedVips(usernameToCheck);

            client.SendMessage(joinedChannel,
                giftedVips == null ?
                $"Hey @{username}, sorry I can't check that at the moment, please try again in a few minutes" :
                $"Hey @{username}, {username} has given out a total of {giftedVips.GiftedVips} to the community!");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command will tell you how many VIPs you have gifted to others, if you provide an @username I'll tell you how generous they are too!");
        }
    }
}