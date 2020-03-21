using System;
using System.Linq;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.Vip;
using CoreCodedChatbot.Interfaces;
using Microsoft.Extensions.Logging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] {"gvip", "givevip"}, true)]
    public class GiveVipCommand : ICommand
    {
        private readonly IVipApiClient _vipApiClient;
        private readonly ILogger<GiveVipCommand> _logger;

        public GiveVipCommand(
            IVipApiClient vipApiClient,
            ILogger<GiveVipCommand> logger)
        {
            _vipApiClient = vipApiClient;
            _logger = logger;
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod,
            JoinedChannel joinedChannel)
        {
            var splitCommandText = commandText.Split(" ");

            if (commandText.Contains("@"))
            {
                var giveVipModel = new ModGiveVipRequest
                {
                    ReceivingUsername = commandText.TrimStart('@'),
                    VipsToGive = 1
                };

                if (splitCommandText.Length == 2)
                {
                    var giveUser = splitCommandText.SingleOrDefault(x => x.Contains("@")).TrimStart('@');

                    if (int.TryParse(splitCommandText.SingleOrDefault(x => !x.Contains("@")), out var giveAmount))
                        giveVipModel = new ModGiveVipRequest
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
                    _logger.LogError($"Error encountered when giving a single VIP", new object [] {username, commandText, isMod});

            }
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel, $"Hey @{username}, this command is used by moderators to give out VIP requests. Hint: Ensure you use the '@'. Usage: !gvip <username> <optionalAmount>");
        }
    }
}
