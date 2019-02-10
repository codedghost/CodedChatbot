using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using Newtonsoft.Json;

using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Helpers.Interfaces;
using CoreCodedChatbot.Library.Models.Data;
using Microsoft.EntityFrameworkCore.Internal;
using TwitchLib.Client.Events;
using TwitchLib.PubSub.Events;
using TwitchLib.Client;
using TwitchLib.Api;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Models;
using TwitchLib.PubSub;

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
        private readonly StreamLabsHelper streamLabsHelper;
        private readonly LiveStreamMonitorService liveStreamMonitor;

        private Timer HowToRequestTimer { get; set; }
        private Timer CustomsForgeTimer { get; set; }
        private Timer DiscordTimer { get; set; }
        private Timer TwitterTimer { get; set; }
        private Timer InstagramTimer { get; set; }
        private Timer BytesTimer { get; set; }
        private Timer DonationsTimer { get; set; }
        private Timer PlaylistTimer { get; set; }
        private Timer YoutubeTimer { get; set; }
        private Timer MerchTimer { get; set; }

        private int MaxTimerMinutesRocksmith = 56;
        private int MaxTimerMinutesGaming = 35;

        private int ChattyTimerCounter = 0;
        private int MinutesBetweenChattyCommands = 7;

        private readonly ConfigModel config;

        private static readonly HttpClient httpClient = new HttpClient();

        private string DevelopmentRoomId = string.Empty; // Only for use in debug mode

        public ChatbotService(CommandHelper commandHelper, TwitchClient client, TwitchAPI api, TwitchPubSub pubsub, LiveStreamMonitorService liveStreamMonitor,
            VipHelper vipHelper, BytesHelper bytesHelper, StreamLabsHelper streamLabsHelper, IConfigHelper configHelper)
        {
            this.commandHelper = commandHelper;
            this.client = client;
            this.api = api;
            this.pubsub = pubsub;
            this.liveStreamMonitor = liveStreamMonitor;
            this.vipHelper = vipHelper;
            this.bytesHelper = bytesHelper;
            this.config = configHelper.GetConfig();
            this.streamLabsHelper = streamLabsHelper;

            this.commandHelper.Init();

            this.client.OnJoinedChannel += OnJoinedChannel;
            this.client.OnChatCommandReceived += OnCommandReceived;
            this.client.OnNewSubscriber += OnNewSub;
            this.client.OnReSubscriber += OnReSub;
            this.client.OnGiftedSubscription += OnGiftSub;
            this.client.OnCommunitySubscription += OnSubBomb;
            this.client.Connect();
            
            this.liveStreamMonitor.SetChannelsByName(new List<string>{config.StreamerChannel});
            this.liveStreamMonitor.OnStreamOnline += OnStreamOnline;
            this.liveStreamMonitor.OnStreamOffline += OnStreamOffline;
            this.liveStreamMonitor.OnServiceStarted += OnStreamMonitorStarted;
            //this.liveStreamMonitor.OnStreamUpdate += OnStreamUpdate;

            this.liveStreamMonitor.Start();

            this.pubsub.OnPubSubServiceConnected += OnPubSubConnected;
            this.pubsub.OnBitsReceived += OnBitsReceived;
            this.pubsub.OnListenResponse += OnListenResponse;

            this.pubsub.Connect();
        }

        private void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {

            if (config.DevelopmentBuild)
            {
                api.V5.Chat.GetChatRoomsByChannelAsync(config.ChannelId, config.ChatbotAccessToken)
                    .ContinueWith(
                        rooms =>
                        {
                            if (!rooms.IsCompletedSuccessfully) return;
                            DevelopmentRoomId = rooms.Result.Rooms.SingleOrDefault(r => r.Name == "dev")?.Id;
                            if (!string.IsNullOrWhiteSpace(DevelopmentRoomId))
                            {
                                client.JoinRoom(config.ChannelId, DevelopmentRoomId);
                                client.SendMessage(client.JoinedChannels.FirstOrDefault(jc => jc.Channel.Contains(DevelopmentRoomId)),
                                    $"BEEP BOOP: {config.ChatbotNick} has joined dev!");
                            }
                        });

                ScheduleStreamTasks(); // If we are in development we should leave chatty tasks running
            }
            else
            {
                client.SendMessage(config.StreamerChannel, $"BEEP BOOP: {config.ChatbotNick} online!");
            }
        }

        private void OnCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            try
            {
                if ((config.DevelopmentBuild && !e.Command.ChatMessage.Channel.Contains(DevelopmentRoomId)) ||
                     (!config.DevelopmentBuild && e.Command.ChatMessage.Channel.Contains(DevelopmentRoomId) && !string.IsNullOrWhiteSpace(DevelopmentRoomId)))
                    return;

                if (config.DevelopmentBuild && !client.JoinedChannels.Select(jc => jc.Channel)
                        .Any(jc => jc.Contains(DevelopmentRoomId)))
                    client.JoinRoom(config.ChannelId, DevelopmentRoomId);

                commandHelper.ProcessCommand(e.Command.CommandText, client, e.Command.ChatMessage.Username,
                    e.Command.ArgumentsAsString,
                    e.Command.ChatMessage.IsModerator || e.Command.ChatMessage.IsBroadcaster,
                    client.JoinedChannels.FirstOrDefault(jc => jc.Channel == e.Command.ChatMessage.Channel));
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.ToString());
            }
        }

        private void OnNewSub(object sender, OnNewSubscriberArgs e)
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

        private void OnReSub(object sender, OnReSubscriberArgs e)
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

        private void OnGiftSub(object sender, OnGiftedSubscriptionArgs e)
        {
            try
            {
                Console.Out.WriteLine($"Gifted Sub! {e.GiftedSubscription.MsgParamRecipientUserName} has received a sub from {e.GiftedSubscription.DisplayName}");

                // A whole vip for the recipient
                vipHelper.GiveSubVip(e.GiftedSubscription.MsgParamRecipientUserName);

                // Half as thanks to the gifter
                bytesHelper.GiveGiftSubBytes(e.GiftedSubscription.DisplayName);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex);
            }
        }

        private void OnSubBomb(object sender, OnCommunitySubscriptionArgs e)
        {
            try
            {
                Console.Out.WriteLine($"Sub Bomb!!! {e.GiftedSubscription.DisplayName} has gifted {e.GiftedSubscription.MsgParamMassGiftCount} subs!");
                
                // Leaving blank for now, it is assumed that subbomb gifts go through gift event too, so relevant vips and bytes will be handled there.
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void OnPubSubConnected(object sender, EventArgs e)
        {
            try
            {
                Console.Out.WriteLine("PubSub Connected!");

                pubsub.ListenToBitsEvents(config.ChannelId);

                pubsub.SendTopics(config.ChatbotAccessToken);
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.ToString());
            }
        }

        private void OnListenResponse(object sender, OnListenResponseArgs e)
        {
            Console.Out.WriteLine(e.Successful
                ? $"Successfully verified listening to topic: {e.Topic}"
                : $"Failed to listen! {e.Topic} - Error: {e.Response.Error}");
        }

        private void OnBitsReceived(object sender, OnBitsReceivedArgs e)
        {
            try
            {
                Console.Out.WriteLine("Bits Dropped :O!");
                Console.Out.WriteLine($"{e.Username} dropped {e.BitsUsed} - Total {e.TotalBitsUsed}");
                if (vipHelper.GiveBitsVip(e.Username, e.TotalBitsUsed))
                    vipHelper.GiveDonationVips(e.Username);

            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.ToString());
            }
        }

        private void OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            Console.Out.WriteLine("Streamer is online");
            JoinedChannel channel = null;

            channel = client.GetJoinedChannel(config.StreamerChannel);
            client.SendMessage(e.Channel, $"Looks like @{e.Channel} has come online, better get to work!");

            ScheduleStreamTasks(e.Stream.Title);
        }

        private void OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            Console.Out.WriteLine("Streamer is offline");

            if (client.IsConnected)
            {
                if (config.DevelopmentBuild)
                {
                    api.V5.Chat.GetChatRoomsByChannelAsync(config.ChannelId, config.ChatbotAccessToken)
                        .ContinueWith(
                            rooms =>
                            {
                                if (!rooms.IsCompletedSuccessfully) return;
                                var devRoomId = rooms.Result.Rooms.SingleOrDefault(r => r.Name == "dev")?.Id;
                                if (!string.IsNullOrWhiteSpace(devRoomId))
                                {
                                    client.JoinRoom(config.ChannelId, devRoomId);
                                    client.SendMessage(client.JoinedChannels.FirstOrDefault(jc => jc.Channel.Contains(devRoomId)),
                                        $"Looks like @{e.Channel} has gone offline, *yawn* powering down");
                                }
                            });
                }
                else
                {
                    client.SendMessage(e.Channel, $"Looks like @{e.Channel} has gone offline, *yawn* powering down");
                }
            }
            UnScheduleStreamTasks();
        }

        private void OnStreamMonitorStarted(object sender, OnServiceStartedArgs e)
        {
            Console.Out.WriteLine("Stream Monitor Started");
            Console.Out.WriteLine($"Monitoring Channels: {e}");
        }

        private void OnStreamUpdate(object sender, OnStreamUpdateArgs e)
        {
            Console.Out.WriteLine("Assuming stream category or title has updated, rescheduling tasks");
            UnScheduleStreamTasks();
            //ScheduleStreamTasks(e.Stream.Title);
        }

        private async void ScheduleStreamTasks(string streamGame = "Rocksmith 2014")
        {
            var isStreamingRocksmith = streamGame == "Rocksmith 2014"; // TODO: This needs to query the actual game id as this currently doesn't work correctly
            var maxTimerMinutes =
                TimeSpan.FromMinutes(isStreamingRocksmith ? MaxTimerMinutesRocksmith : MaxTimerMinutesGaming);

            // Align database with any potentially missed or offline subs
            try
            {
                var subs = await api.V5.Channels.GetAllSubscribersAsync(config.ChannelId, config.ChatbotAccessToken);

                // TODO: Need to consider length of sub in db alignment
                vipHelper.StartupSubVips(subs);
            }
            catch (NotPartneredException)
            {
                Console.Out.WriteLine("Not a partner. Skipping sub setup.");
            }

            var roomId = config.DevelopmentBuild ? DevelopmentRoomId : config.ChannelId;
            if (config.DevelopmentBuild && !client.JoinedChannels.Any(jc => jc.Channel.Contains(DevelopmentRoomId)))
                client.JoinRoom(config.ChannelId, DevelopmentRoomId);

            var joinedRoom = client.JoinedChannels.FirstOrDefault(jc => jc.Channel.Contains(roomId));
            // Set threads for sending out stream info to the chat.
            if (isStreamingRocksmith)
            {
                HowToRequestTimer = new Timer(
                    e => commandHelper.ProcessCommand("howtorequest", client, "Chatbot", string.Empty, true, joinedRoom),
                    null,
                    AssignChattyTimer(), maxTimerMinutes);
                CustomsForgeTimer = new Timer(
                    e => commandHelper.ProcessCommand("customsforge", client, "Chatbot", string.Empty, true, joinedRoom),
                    null,
                    AssignChattyTimer(), maxTimerMinutes);
                PlaylistTimer = new Timer(
                    e => commandHelper.ProcessCommand("list", client, "Chatbot", string.Empty, true, joinedRoom),
                    null,
                    AssignChattyTimer(), maxTimerMinutes);
            }
            
            DiscordTimer = new Timer(
                e => commandHelper.ProcessCommand("discord", client, "Chatbot", string.Empty, true, joinedRoom),
                null,
                AssignChattyTimer(), maxTimerMinutes);
            InstagramTimer = new Timer(
                e => commandHelper.ProcessCommand("instagram", client, "Chatbot", string.Empty, true, joinedRoom),
                null,
                AssignChattyTimer(), maxTimerMinutes);
            TwitterTimer = new Timer(
                e => commandHelper.ProcessCommand("twitter", client, "Chatbot", string.Empty, true, joinedRoom),
                null,
                AssignChattyTimer(), maxTimerMinutes);
            YoutubeTimer = new Timer(
                e => commandHelper.ProcessCommand("youtube", client, "Chatbot", string.Empty, true, joinedRoom),
                null,
                AssignChattyTimer(), maxTimerMinutes);
            
            MerchTimer = new Timer(
                e => commandHelper.ProcessCommand("merch", client, "Chatbot", string.Empty, true, joinedRoom),
                null,
                AssignChattyTimer(), maxTimerMinutes);


            // Set thread for checking viewers in chat and giving out Bytes.

            BytesTimer = new Timer(
                async e =>
                {
                    try
                    {
                        var currentChattersJson = await httpClient.GetAsync($"https://tmi.twitch.tv/group/user/{config.StreamerChannel}/chatters");

                        if (currentChattersJson.IsSuccessStatusCode)
                        {
                            // process json into username list.
                            var chattersModel =
                                JsonConvert.DeserializeObject<ChatViewersModel>(currentChattersJson.Content
                                    .ReadAsStringAsync().Result);
                            bytesHelper.GiveViewershipBytes(chattersModel);
                        }
                        else Console.Out.WriteLine("Could not retrieve Chatters JSON");
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
            
            DonationsTimer = new System.Threading.Timer(
                e =>
                {
                    try
                    {
                        var success = streamLabsHelper.CheckDonationVips();
                    }
                    catch (Exception ex)
                    {
                        Console.Out.WriteLine(ex.ToString());
                    }
                },
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(1));
        }

        private void UnScheduleStreamTasks()
        {
            HowToRequestTimer?.Dispose();
            CustomsForgeTimer?.Dispose();
            PlaylistTimer?.Dispose();
            DiscordTimer?.Dispose();
            InstagramTimer?.Dispose();
            TwitterTimer?.Dispose();
            YoutubeTimer?.Dispose();
            MerchTimer?.Dispose();
            BytesTimer?.Dispose();
            DonationsTimer?.Dispose();
        }

        private TimeSpan AssignChattyTimer()
        {
            var timer = TimeSpan.FromMinutes(ChattyTimerCounter);
            ChattyTimerCounter += MinutesBetweenChattyCommands;

            return timer;
        }

        public void Main()
        {
        }
    }
}
