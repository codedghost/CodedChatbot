using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Models.ApiRequest.Vip;
using CoreCodedChatbot.Library.Models.Data;

using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] {"gvip", "givevip"}, true)]
    public class GiveVipCommand : ICommand
    {
        private readonly IVipApiClient _vipApiClient;

        public GiveVipCommand(IVipApiClient vipApiClient)
        {
            _vipApiClient = vipApiClient;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod,
            JoinedChannel joinedChannel)
        {
            var splitCommandText = commandText.Split(" ");

            if (commandText.Contains("@"))
            {
                var giveVipModel = new ModGiveVipModel
                {
                    ReceivingUsername = commandText.TrimStart('@'),
                    VipsToGive = 1
                };

                if (splitCommandText.Length == 2)
                {
                    var giveUser = splitCommandText.SingleOrDefault(x => x.Contains("@")).TrimStart('@');

                    if (int.TryParse(splitCommandText.SingleOrDefault(x => !x.Contains("@")), out var giveAmount))
                        giveVipModel = new ModGiveVipModel
                        {
                            ReceivingUsername = giveUser,
                            VipsToGive = giveAmount
                        };
                }

                var result = await _vipApiClient.ModGiveVip(giveVipModel);

                client.SendMessage(joinedChannel,
                    result
                        ? $"Hey @{username}, I have successfully given {giveVipModel.ReceivingUsername} {giveVipModel.VipsToGive} VIPs!"
                        : $"Hey @{username}, sorry something seems to be wrong here. Please check your command usage. Type !help gvip for more detailed help");

                if (!result)
                    Console.Error.WriteLine($"Error encountered when giving a single VIP");

            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command is used by moderators to give out VIP requests. Hint: Ensure you use the '@'. Usage: !gvip <username> <optionalAmount>");
        }
    }
}
