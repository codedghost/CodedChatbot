using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;

using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Models.Data;

using TwitchLib;
using TwitchLib.Models.Client;
using TwitchLib.Events.Client;
using TwitchLib.Events.PubSub;
using TwitchLib.Events.Services.FollowerService;
using TwitchLib.Models.API.v5.Channels;
using TwitchLib.Services;


namespace CoreCodedChatbot.Services
{
    public class ChatbotService
    {
        private readonly CommandHelper commandHelper;
        private readonly TwitchClient client;
        private readonly TwitchAPI api;
        private readonly TwitchPubSub pubsub;
        private readonly FollowerService followerService;
        private readonly VipHelper vipHelper;
        private readonly BytesHelper bytesHelper;

        private ChannelAuthed Channel { get; set; }

        private Timer HowToRequestTimer { get; set; }
        private Timer CustomsForgeTimer { get; set; }
        private Timer DiscordTimer { get; set; }
        private Timer TwitterTimer { get; set; }
        private Timer FollowTimer { get; set; }
        private Timer BytesTimer { get; set; }
        private Timer DonationsTimer { get; set; }

        private ConfigModel config = ConfigHelper.GetConfig();

        private static readonly HttpClient httpClient = new HttpClient();

        public ChatbotService(CommandHelper commandHelper, TwitchClient client, TwitchAPI api, TwitchPubSub pubsub, FollowerService followerService, VipHelper vipHelper, BytesHelper bytesHelper)
        {
            this.commandHelper = commandHelper;
            this.client = client;
            this.api = api;
            this.pubsub = pubsub;
            this.followerService = followerService;
            this.vipHelper = vipHelper;
            this.bytesHelper = bytesHelper;

            this.commandHelper.Init();

            client.OnJoinedChannel += onJoinedChannel;
            client.OnChatCommandReceived += onCommandReceived;
            client.OnNewSubscriber += onNewSub;
            client.OnReSubscriber += onReSub;
            client.Connect();

            pubsub.OnPubSubServiceConnected += onPubSubConnected;
            pubsub.OnListenResponse += onListenResponse;

            // followerService.OnNewFollowersDetected += onNewFollowers;

            pubsub.Connect();
        }

        private void onJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            client.SendMessage($"BEEP BOOP: {config.ChatbotNick} online!");
        }

        private void onCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            try
            {
                commandHelper.ProcessCommand(e.Command.CommandText, client, e.Command.ChatMessage.Username,
                    e.Command.ArgumentsAsString, e.Command.ChatMessage.IsModerator || e.Command.ChatMessage.IsBroadcaster);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.ToString());
            }
        }

        private void onNewSub(object sender, OnNewSubscriberArgs e)
        {
            try
            {
                Console.Out.WriteLine("New Sub! WOOOOO");
                Console.Out.WriteLine(e.Subscriber.DisplayName);
                vipHelper.GiveSubVip(e.Subscriber.DisplayName);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.ToString());
            }
        }

        private void onReSub(object sender, OnReSubscriberArgs e)
        {
            try
            {
                Console.Out.WriteLine("ReSub!!! WOOOOO");
                Console.Out.WriteLine(e.ReSubscriber.DisplayName);
                vipHelper.GiveSubVip(e.ReSubscriber.DisplayName);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.ToString());
            }
        }

        private async void onPubSubConnected(object sender, object e)
        {
            try
            {
                Console.Out.WriteLine("PubSub Connected!");
                Channel = await api.Channels.v5.GetChannelAsync(config.ChatbotAccessToken);

                followerService.SetChannelByChannelId(Channel.Id);
                await followerService.StartService();
                pubsub.ListenToBitsEvents(Channel.Id);
                pubsub.OnBitsReceived += onBitsReceived;
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.ToString());
            }
        }

        private void onListenResponse(object sender, OnListenResponseArgs e)
        {
            Console.Out.WriteLine(e.Successful
                ? $"Successfully verified listening to topic: {e.Topic}"
                : $"Failed to listen! Error: {e.Response.Error}");
        }

        private void onBitsReceived(object sender, OnBitsReceivedArgs e)
        {
            try
            {
                Console.Out.WriteLine("Bits Dropped :O!");
                Console.Out.WriteLine($"{e.Username} - {e.TotalBitsUsed}");
                var vipGiven = vipHelper.GiveDonationVip(e.Username, e.TotalBitsUsed);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.ToString());
            }
        }

        public async void Main()
        {
            var firstRun = true;
            while (true)
            {
                if (firstRun && Channel?.Id != null)
                {
                    // Align database with any potentially missed or offline follows/subs
                    // var followers = await api.Channels.v5.GetAllFollowersAsync(Channel.Id);
                    var subs = await api.Channels.v5.GetAllSubscribersAsync(Channel.Id, config.ChatbotAccessToken);

                    // VipHelper.StartupFollowVips(followers);
                    // TODO: Need to consider length of sub in db alignment
                    vipHelper.StartupSubVips(subs);

                    // Set threads for sending out stream info to the chat.
                    HowToRequestTimer = new Timer(
                        e => commandHelper.ProcessCommand("howtorequest", client, "Chatbot", string.Empty, true),
                        null,
                        TimeSpan.Zero,
                        TimeSpan.FromMinutes(25));
                    CustomsForgeTimer = new Timer(
                        e => commandHelper.ProcessCommand("customsforge", client, "Chatbot", string.Empty, true),
                        null,
                        TimeSpan.FromMinutes(5),
                        TimeSpan.FromMinutes(25));
                    FollowTimer = new Timer(
                        e => commandHelper.ProcessCommand("followme", client, "Chatbot", string.Empty, true),
                        null,
                        TimeSpan.FromMinutes(10),
                        TimeSpan.FromMinutes(25));
                    DiscordTimer = new Timer(
                        e => commandHelper.ProcessCommand("discord", client, "Chatbot", string.Empty, true),
                        null,
                        TimeSpan.FromMinutes(15),
                        TimeSpan.FromMinutes(25));
                    TwitterTimer = new Timer(
                        e => commandHelper.ProcessCommand("twitter", client, "Chatbot", string.Empty, true),
                        null,
                        TimeSpan.FromMinutes(20),
                        TimeSpan.FromMinutes(25));

                    // Set thread for checking viewers in chat and giving out Bytes.

                    BytesTimer = new Timer(
                        async e =>
                        {
                            try
                            {
                                var currentChattersJson = await httpClient.GetStringAsync($"https://tmi.twitch.tv/group/user/{config.StreamerChannel}/chatters");
                                // process json into username list.
                                var chattersModel = JsonConvert.DeserializeObject<ChatViewersModel>(currentChattersJson);
                                Console.Out.WriteLine(currentChattersJson);
                                bytesHelper.GiveBytes(chattersModel);
                                Console.Out.WriteLine(PlaylistHelper.GetEstimatedTime(chattersModel));
                            }
                            catch (Exception ex)
                            {
                                Console.Out.WriteLine(ex.ToString());
                            }
                        },
                        null,
                        TimeSpan.Zero,
                        TimeSpan.FromMinutes(1));

                    // Set thread for checking for any new Donations from streamlabs and synchronise with the db.
                    // Commenting out so bot can be used.
                    /*
                    DonationsTimer = new System.Threading.Timer(
                        async e =>
                        {
                            try
                            {
                                var vals = new Dictionary<string, string>
                                {
                                    { "grant_type", "authorization_code" },
                                    { "client_id", config.StreamLabsClientId },
                                    { "client_secret", config.StreamLabsClientSecret },
                                    { "redirect_uri", "localhost" },
                                    { "code", config.StreamLabsCode }
                                };
                                var content = new FormUrlEncodedContent(vals);
                                var tokenResponse = await httpClient.PostAsync("https://streamlabs.com/api/v1.0/token", content);
                                var tokenJsonString = await tokenResponse.Content.ReadAsStringAsync();
                                var tokenModel = JsonConvert.DeserializeObject<TokenJsonModel>(tokenJsonString);



                                var donationsJsonString = await httpClient.GetStringAsync($"https://streamlabs.com/api/v1.0/donations?access_token={tokenModel.access_token}");
                                var donationsModel = JsonConvert.DeserializeObject<TokenJsonModel>(donationsJsonString);
                                var text = "";

                                // NOTES TO SELF. Add &after=<donationId> to end of donations call. 
                                // This can start at any number (for initialising db) after this it will get anything past whichever donation_id we pass
                                // This will ideally always be the latest one, we can then check for empty strings and such to reduce overhead.
                            }
                            catch (Exception ex)
                            {
                                Console.Out.WriteLine(ex.ToString());
                            }
                        },
                        null,
                        TimeSpan.Zero,
                        TimeSpan.FromMinutes(1));
                    */

                    firstRun = false;
                    break;
                }
            }
        }
    }
}
