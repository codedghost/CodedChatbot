using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CoreCodedChatbot.Config;
using Newtonsoft.Json;

using CoreCodedChatbot.Helpers;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Secrets;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using TwitchLib.Client.Events;
using TwitchLib.PubSub.Events;
using TwitchLib.Client;
using TwitchLib.Api;
using TwitchLib.Api.Core.Exceptions;
using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Client.Interfaces;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Events;
using TwitchLib.PubSub;
using TwitchLib.PubSub.Interfaces;

namespace CoreCodedChatbot.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly ICommandHelper _commandHelper;
        private readonly TwitchClient _client;
        private readonly TwitchAPI _api;
        private readonly TwitchPubSub _pubsub;
        private readonly IVipHelper _vipHelper;
        private readonly IBytesHelper _bytesHelper;
        private readonly IStreamLabsHelper _streamLabsHelper;
        private readonly IConfigService _configService;
        private readonly ISecretService _secretService;
        private readonly ILogger<ChatbotService> _logger;
        private readonly LiveStreamMonitorService _liveStreamMonitor;

        private readonly string _streamerChannel;
        private readonly bool _isDevelopmentBuild;

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
        private Timer RocksmithChallengeTimer { get; set; }
        private Timer ChatConnectionTimer { get; set; }

        private int _maxTimerMinutesRocksmith = 135;
        private int _maxTimerMinutesGaming = 90;

        private int _chattyTimerCounter = 0;
        private int _minutesBetweenChattyCommands = 15;

        private static readonly HttpClient _httpClient = new HttpClient();

        private readonly string _developmentRoomId = string.Empty; // Only for use in debug mode

        public ChatbotService(ICommandHelper commandHelper, TwitchClient client, TwitchAPI api, 
            TwitchPubSub pubsub, LiveStreamMonitorService liveStreamMonitor,
            IVipHelper vipHelper, IBytesHelper bytesHelper, IStreamLabsHelper streamLabsHelper, 
            IConfigService configService, ISecretService secretService,
            ILogger<ChatbotService> logger)
        {
            _commandHelper = commandHelper;
            _client = client;
            _api = api;
            _pubsub = pubsub;
            _liveStreamMonitor = liveStreamMonitor;
            _vipHelper = vipHelper;
            _bytesHelper = bytesHelper;
            _streamLabsHelper = streamLabsHelper;
            _configService = configService;
            _secretService = secretService;
            _logger = logger;

            _streamerChannel = _configService.Get<string>("StreamerChannel");
            _isDevelopmentBuild = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ||
                                  Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Local";

            _client.OnJoinedChannel += OnJoinedChannel;
            _client.OnChatCommandReceived += OnCommandReceived;
            _client.OnNewSubscriber += OnNewSub;
            _client.OnReSubscriber += OnReSub;
            _client.OnGiftedSubscription += OnGiftSub;
            _client.OnCommunitySubscription += OnSubBomb;
            _client.OnBeingHosted += OnBeingHosted;
            _client.OnRaidNotification += OnRaidNotification;
            _client.OnDisconnected += OnDisconnected;
            _client.OnError += OnError;
            _client.Connect();
            
            _liveStreamMonitor.SetChannelsByName(new List<string>{_streamerChannel});
            _liveStreamMonitor.OnStreamOnline += OnStreamOnline;
            _liveStreamMonitor.OnStreamOffline += OnStreamOffline;
            _liveStreamMonitor.OnServiceStarted += OnStreamMonitorStarted;
            //this.liveStreamMonitor.OnStreamUpdate += OnStreamUpdate;

            _liveStreamMonitor.Start();

            _pubsub.OnPubSubServiceConnected += OnPubSubConnected;
            _pubsub.OnBitsReceived += OnBitsReceived;
            _pubsub.OnListenResponse += OnListenResponse;

            _pubsub.Connect();
        }

        private void JoinChannel()
        {
            if (_isDevelopmentBuild)
            {
                //api.V5.Chat.GetChatRoomsByChannelAsync(config.ChannelId, config.ChatbotAccessToken)
                //    .ContinueWith(
                //        rooms =>
                //        {
                //            if (!rooms.IsCompletedSuccessfully) return;
                //            DevelopmentRoomId = rooms.Result.Rooms.SingleOrDefault(r => r.Name == "dev")?.Id;
                //            if (!string.IsNullOrWhiteSpace(DevelopmentRoomId))
                //            {
                //                client.JoinRoom(config.ChannelId, DevelopmentRoomId);
                //                client.SendMessage(client.JoinedChannels.FirstOrDefault(jc => jc.Channel.Contains(DevelopmentRoomId)),
                //                    $"BEEP BOOP: {config.ChatbotNick} has joined dev!");
                //            }
                //        });

                ScheduleStreamTasks(); // If we are in development we should leave chatty tasks running
            }
            else
            {
                _client.SendMessage(_streamerChannel, $"BEEP BOOP: {_configService.Get<string>("ChatbotNick")} online!");
            }
        }

        private void OnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            JoinChannel();
        }

        private void OnCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            try
            {
                if (
                    (_isDevelopmentBuild && !e.Command.ChatMessage.Channel.Contains(_developmentRoomId))
                    || (!_isDevelopmentBuild && e.Command.ChatMessage.Channel.Contains(_developmentRoomId)
                        && !string.IsNullOrWhiteSpace(_developmentRoomId)))
                {
                    return;
                }

                _commandHelper.ProcessCommand(
                    e.Command.CommandText,
                    _client,
                    e.Command.ChatMessage.Username,
                    e.Command.ArgumentsAsString,
                    e.Command.ChatMessage.IsModerator || e.Command.ChatMessage.IsBroadcaster,
                    _client.JoinedChannels.FirstOrDefault(jc => jc.Channel == e.Command.ChatMessage.Channel));
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
                _vipHelper.GiveSubVip(e.Subscriber.DisplayName);
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
                _vipHelper.GiveSubVip(e.ReSubscriber.DisplayName, e.ReSubscriber.Months);
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
                _vipHelper.GiveSubVip(e.GiftedSubscription.MsgParamRecipientUserName);

                // Half as thanks to the gifter
                _bytesHelper.GiveGiftSubBytes(e.GiftedSubscription.DisplayName);
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

                _pubsub.ListenToBitsEvents(_configService.Get<string>("ChannelId"));

                _pubsub.SendTopics(_secretService.GetSecret<string>("ChatbotAccessToken"));
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
                if (_vipHelper.GiveBitsVip(e.Username, e.TotalBitsUsed))
                    _vipHelper.GiveDonationVips(e.Username);

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

            channel = _client.GetJoinedChannel(_streamerChannel);
            _client.SendMessage(channel.Channel, $"Looks like @{channel.Channel} has come online, better get to work!");

            ScheduleStreamTasks(e.Stream.Title);
        }

        private void OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            Console.Out.WriteLine("Streamer is offline");

            if (_client.IsConnected)
            {
                _client.SendMessage(e.Channel, $"Looks like @{e.Channel} has gone offline, *yawn* powering down");
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

        private void OnRaidNotification(object sender, OnRaidNotificationArgs e)
        {
            int.TryParse(e.RaidNotification.MsgParamViewerCount, out var viewersCount);
            WelcomeRaidOrHost(e.Channel, e.RaidNotification.MsgParamDisplayName, viewersCount, true);
        }

        private void OnBeingHosted(object sender, OnBeingHostedArgs e)
        {
            WelcomeRaidOrHost(e.BeingHostedNotification.Channel, e.BeingHostedNotification.HostedByChannel, e.BeingHostedNotification.Viewers, false);
        }

        private void WelcomeRaidOrHost(string hostedChannelName, string username, int numberofRaiders, bool isRaid)
        {
            var typeText = isRaid ? "raid" : "host";
            _client.SendMessage(hostedChannelName,
                $"Hey everyone, we're getting a {typeText} from @{username} with {numberofRaiders} of their friends! Welcome one and all! codedgUitar");
        }

        private async void ScheduleStreamTasks(string streamGame = "Rocksmith 2014")
        {
            var isStreamingRocksmith = streamGame == "Rocksmith 2014"; // TODO: This needs to query the actual game id as this currently doesn't work correctly
            var maxTimerMinutes =
                TimeSpan.FromMinutes(isStreamingRocksmith ? _maxTimerMinutesRocksmith : _maxTimerMinutesGaming);

            // Align database with any potentially missed or offline subs
            try
            {
                var subs = await _api.V5.Channels.GetAllSubscribersAsync(_configService.Get<string>("ChannelId"),
                    _secretService.GetSecret<string>("ChatbotAccessToken"));

                // TODO: Need to consider length of sub in db alignment
                _vipHelper.StartupSubVips(subs);
            }
            catch (NotPartneredException)
            {
                Console.Out.WriteLine("Not a partner. Skipping sub setup.");
            }
            

            var joinedRoom = _client.JoinedChannels.FirstOrDefault(jc =>
                // config.DevelopmentBuild ? jc.Channel.Contains(DevelopmentRoomId) :
                jc.Channel == _configService.Get<string>("StreamerChannel"));
            // Set threads for sending out stream info to the chat.
            if (isStreamingRocksmith)
            {
                HowToRequestTimer = new Timer(
                    e => _commandHelper.ProcessCommand("howtorequest", _client, "Chatbot", string.Empty, true, joinedRoom),
                    null,
                    AssignChattyTimer(), maxTimerMinutes);
                CustomsForgeTimer = new Timer(
                    e => _commandHelper.ProcessCommand("customsforge", _client, "Chatbot", string.Empty, true, joinedRoom),
                    null,
                    AssignChattyTimer(), maxTimerMinutes);
                PlaylistTimer = new Timer(
                    e => _commandHelper.ProcessCommand("list", _client, "Chatbot", string.Empty, true, joinedRoom),
                    null,
                    AssignChattyTimer(), maxTimerMinutes);
            }
            
            DiscordTimer = new Timer(
                e => _commandHelper.ProcessCommand("discord", _client, "Chatbot", string.Empty, true, joinedRoom),
                null,
                AssignChattyTimer(), maxTimerMinutes);
            InstagramTimer = new Timer(
                e => _commandHelper.ProcessCommand("instagram", _client, "Chatbot", string.Empty, true, joinedRoom),
                null,
                AssignChattyTimer(), maxTimerMinutes);
            TwitterTimer = new Timer(
                e => _commandHelper.ProcessCommand("twitter", _client, "Chatbot", string.Empty, true, joinedRoom),
                null,
                AssignChattyTimer(), maxTimerMinutes);
            YoutubeTimer = new Timer(
                e => _commandHelper.ProcessCommand("youtube", _client, "Chatbot", string.Empty, true, joinedRoom),
                null,
                AssignChattyTimer(), maxTimerMinutes);
            RocksmithChallengeTimer = new Timer(
                e => _commandHelper.ProcessCommand("challenge", _client, "Chatbot", string.Empty, true, joinedRoom),
                null,
                AssignChattyTimer(), maxTimerMinutes);
            MerchTimer = new Timer(
                e => _commandHelper.ProcessCommand("merch", _client, "Chatbot", string.Empty, true, joinedRoom),
                null,
                AssignChattyTimer(), maxTimerMinutes);


            // Set thread for checking viewers in chat and giving out Bytes.

            BytesTimer = new Timer(
                async e =>
                {
                    try
                    {
                        var currentChattersJson = await _httpClient.GetAsync($"https://tmi.twitch.tv/group/user/{_streamerChannel}/chatters");

                        if (currentChattersJson.IsSuccessStatusCode)
                        {
                            // process json into username list.
                            var chattersModel =
                                JsonConvert.DeserializeObject<ChatViewersModel>(currentChattersJson.Content
                                    .ReadAsStringAsync().Result);
                            _bytesHelper.GiveViewershipBytes(chattersModel);
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
                        var success = _streamLabsHelper.CheckDonationVips();
                    }
                    catch (Exception ex)
                    {
                        Console.Out.WriteLine(ex.ToString());
                    }
                },
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(1));

            ChatConnectionTimer = new Timer(
                async e =>
                {
                    try
                    {
                        var currentChattersJson = await _httpClient.GetAsync($"https://tmi.twitch.tv/group/user/{_configService.Get<string>("StreamerChannel")}/chatters");

                        if (currentChattersJson.IsSuccessStatusCode)
                        {
                            // process json into username list.
                            var chattersModel =
                                JsonConvert.DeserializeObject<ChatViewersModel>(currentChattersJson.Content
                                    .ReadAsStringAsync().Result);

                            if (chattersModel.chatters.moderators.Contains(_configService.Get<string>("ChatbotNick"))) return;

                            Console.Error.WriteLine($"DISCONNECTED FROM CHAT, RECONNECTING");

                            _client.Connect();
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
                TimeSpan.FromSeconds(10));
        }
        
        private void OnError(object sender, OnErrorEventArgs e)
        {
            Console.Error.WriteLine($"EXCEPTION - {DateTime.Now} \n Exception:\n{e.Exception} \n\n Inner: {e.Exception.InnerException}");
        }

        private void OnDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            Console.Error.WriteLine($"DISCONNECTED FROM CHAT");
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
            RocksmithChallengeTimer?.Dispose();
            BytesTimer?.Dispose();
            DonationsTimer?.Dispose();
        }

        private TimeSpan AssignChattyTimer()
        {
            var timer = TimeSpan.FromMinutes(_chattyTimerCounter);
            _chattyTimerCounter += _minutesBetweenChattyCommands;

            return timer;
        }

        public void Main()
        {
        }
    }
}
