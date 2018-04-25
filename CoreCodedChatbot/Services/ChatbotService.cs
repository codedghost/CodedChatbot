using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;

using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Models.Data;

using TwitchLib.Client.Events;
using TwitchLib.PubSub.Events;
using TwitchLib.Api.Exceptions;
using TwitchLib.Client;
using TwitchLib.Api;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.PubSub;
using TwitchLib.Api.Models.v5.Channels;

namespace CoreCodedChatbot.Services
{
    public class ChatbotService
    {
        private readonly CommandHelper commandHelper;
        private readonly TwitchClient client;
        private readonly TwitchAPI api;
        private readonly TwitchPubSub pubsub;
        private readonly VipHelper vipHelper;
        private readonly BytesHelper bytesHelper;
        private readonly PlaylistHelper playlistHelper;
        private readonly LiveStreamMonitor liveStreamMonitor;

        private ChannelAuthed Channel { get; set; }

        private Timer HowToRequestTimer { get; set; }
        private Timer CustomsForgeTimer { get; set; }
        private Timer DiscordTimer { get; set; }
        private Timer TwitterTimer { get; set; }
        private Timer FollowTimer { get; set; }
        private Timer BytesTimer { get; set; }
        private Timer DonationsTimer { get; set; }

        private ConfigModel config;

        private static readonly HttpClient httpClient = new HttpClient();

        public ChatbotService(CommandHelper commandHelper, TwitchClient client, TwitchAPI api, TwitchPubSub pubsub, LiveStreamMonitor liveStreamMonitor,
            VipHelper vipHelper, BytesHelper bytesHelper, PlaylistHelper playlistHelper, ConfigModel config)
        {
            this.commandHelper = commandHelper;
            this.client = client;
            this.api = api;
            this.pubsub = pubsub;
            this.liveStreamMonitor = liveStreamMonitor;
            this.vipHelper = vipHelper;
            this.bytesHelper = bytesHelper;
            this.playlistHelper = playlistHelper;
            this.config = config;

            this.commandHelper.Init();

            this.client.OnJoinedChannel += onJoinedChannel;
            this.client.OnChatCommandReceived += onCommandReceived;
            this.client.OnNewSubscriber += onNewSub;
            this.client.OnReSubscriber += onReSub;
            this.client.Connect();

            this.liveStreamMonitor.SetStreamsByUserId(new List<string>{config.ChannelId});
            this.liveStreamMonitor.OnStreamOnline += onStreamOnline;
            this.liveStreamMonitor.OnStreamOffline += onStreamOffline;

            this.liveStreamMonitor.StartService();

            this.pubsub.OnPubSubServiceConnected += onPubSubConnected;
            this.pubsub.OnBitsReceived += onBitsReceived;
            this.pubsub.OnListenResponse += onListenResponse;

            this.pubsub.Connect();
        }

        private void onJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            client.SendMessage(config.StreamerChannel, $"BEEP BOOP: {config.ChatbotNick} online!");
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
                vipHelper.GiveSubVip(e.ReSubscriber.DisplayName, e.ReSubscriber.Months);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.ToString());
            }
        }

        private void onPubSubConnected(object sender, EventArgs e)
        {
            try
            {
                Console.Out.WriteLine("PubSub Connected!");
                pubsub.ListenToBitsEvents(config.ChannelId);

                pubsub.SendTopics();
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.ToString());
            }
        }


        private void onWhisperResponse(object sender, OnWhisperArgs e)
        {
            Console.Out.WriteLine(e.Whisper.Data);
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
                Console.Out.WriteLine($"{e.Username} dropped {e.BitsUsed} - Total {e.TotalBitsUsed}");
                var vipGiven = vipHelper.GiveDonationVip(e.Username, e.TotalBitsUsed);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.ToString());
            }
        }

        private async void ScheduleStreamTasks()
        {

            // Align database with any potentially missed or offline subs
            try
            {
                var subs = await api.Channels.v5.GetAllSubscribersAsync(Channel.Id, config.ChatbotAccessToken);

                // TODO: Need to consider length of sub in db alignment
                vipHelper.StartupSubVips(subs);
            }
            catch (NotPartneredException)
            {
                Console.Out.WriteLine("Not a partner. Skipping sub setup.");
            }

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
                        Console.Out.WriteLine(playlistHelper.GetEstimatedTime(chattersModel));
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

        }

        private void UnScheduleStreamTasks()
        {
            HowToRequestTimer.Dispose();
            CustomsForgeTimer.Dispose();
            FollowTimer.Dispose();
            DiscordTimer.Dispose();
            TwitterTimer.Dispose();
            BytesTimer.Dispose();
            DonationsTimer.Dispose();
        }

        private void onStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            client.SendMessage(e.Channel, $"Looks like @{e.Channel} has come online, better get to work!");
            ScheduleStreamTasks();
        }

        private void onStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            client.SendMessage(e.Channel, $"Looks like @{e.Channel} has gone offline, *yawn* powering down");
            UnScheduleStreamTasks();
        }

        public void Main()
        {
            var firstRun = true;
            while (true)
            {
                if (firstRun && Channel?.Id != null)
                {
                    ScheduleStreamTasks();

                    firstRun = false;
                    break;
                }
            }
        }
    }
}
