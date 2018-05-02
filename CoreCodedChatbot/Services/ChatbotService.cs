﻿using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;

using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Helpers.Interfaces;
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
        private readonly StreamLabsHelper streamLabsHelper;
        private readonly LiveStreamMonitor liveStreamMonitor;

        private Timer HowToRequestTimer { get; set; }
        private Timer CustomsForgeTimer { get; set; }
        private Timer DiscordTimer { get; set; }
        private Timer TwitterTimer { get; set; }
        private Timer FollowTimer { get; set; }
        private Timer BytesTimer { get; set; }
        private Timer DonationsTimer { get; set; }

        private readonly ConfigModel config;

        private static readonly HttpClient httpClient = new HttpClient();

        public ChatbotService(CommandHelper commandHelper, TwitchClient client, TwitchAPI api, TwitchPubSub pubsub, LiveStreamMonitor liveStreamMonitor,
            VipHelper vipHelper, BytesHelper bytesHelper, PlaylistHelper playlistHelper, StreamLabsHelper streamLabsHelper, IConfigHelper configHelper)
        {
            this.commandHelper = commandHelper;
            this.client = client;
            this.api = api;
            this.pubsub = pubsub;
            this.liveStreamMonitor = liveStreamMonitor;
            this.vipHelper = vipHelper;
            this.bytesHelper = bytesHelper;
            this.playlistHelper = playlistHelper;
            this.config = configHelper.GetConfig();
            this.streamLabsHelper = streamLabsHelper;

            this.commandHelper.Init();

            this.client.OnJoinedChannel += OnJoinedChannel;
            this.client.OnChatCommandReceived += OnCommandReceived;
            this.client.OnNewSubscriber += OnNewSub;
            this.client.OnReSubscriber += OnReSub;
            this.client.Connect();

            this.liveStreamMonitor.SetStreamsByUserId(new List<string>{config.ChannelId});
            this.liveStreamMonitor.OnStreamOnline += OnStreamOnline;
            this.liveStreamMonitor.OnStreamOffline += OnStreamOffline;

            this.liveStreamMonitor.StartService();

            this.pubsub.OnPubSubServiceConnected += OnPubSubConnected;
            this.pubsub.OnBitsReceived += OnBitsReceived;
            this.pubsub.OnListenResponse += OnListenResponse;

            this.pubsub.Connect();
        }

        private void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            client.SendMessage(config.StreamerChannel, $"BEEP BOOP: {config.ChatbotNick} online!");
        }

        private void OnCommandReceived(object sender, OnChatCommandReceivedArgs e)
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
            if (client.IsConnected)
            {
                client.SendMessage(e.Channel, $"Looks like @{e.Channel} has come online, better get to work!");
            }
            ScheduleStreamTasks();
        }

        private void OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            if (client.IsConnected)
            {
                client.SendMessage(e.Channel, $"Looks like @{e.Channel} has gone offline, *yawn* powering down");
            }
            UnScheduleStreamTasks();
        }

        private async void ScheduleStreamTasks()
        {

            // Align database with any potentially missed or offline subs
            try
            {
                var subs = await api.Channels.v5.GetAllSubscribersAsync(config.ChannelId, config.ChatbotAccessToken);

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
            
            DonationsTimer = new System.Threading.Timer(
                e =>
                {
                    try
                    {
                        var success = streamLabsHelper.CheckDonationVips();

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

        public void Main()
        {
        }
    }
}
