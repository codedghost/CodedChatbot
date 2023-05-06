using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CodedChatbot.TwitchFactories.Interfaces;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.ApiContract.RequestModels.StreamStatus;
using CoreCodedChatbot.ApiContract.RequestModels.Vip;
using CoreCodedChatbot.ApiContract.RequestModels.Vip.ChildModels;
using CoreCodedChatbot.ApiContract.SharedExternalRequestModels;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Helpers;
using Newtonsoft.Json;

using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Secrets;
using Microsoft.Extensions.Logging;
using TwitchLib.Api;
using TwitchLib.Client.Events;
using TwitchLib.PubSub.Events;
using TwitchLib.Client;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Communication.Events;
using TwitchLib.PubSub;

namespace CoreCodedChatbot.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly ICommandHelper _commandHelper;
        private readonly ITwitchClientFactory _twitchClientFactory;
        private readonly TwitchPubSub _pubsub;
        private readonly ITwitchLiveStreamMonitorFactory _twitchLiveStreamMonitorFactory;
        private readonly IVipApiClient _vipApiClient;
        private readonly IConfigService _configService;
        private readonly IStreamStatusApiClient _streamStatusApiClient;
        private readonly ISecretService _secretService; 
        private readonly ILogger<ChatbotService> _logger;
        private TwitchClient _client;

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
        //private Timer MerchTimer { get; set; }
        private Timer ShredFestTimer { get; set; }
        private Timer LegatorTimer { get; set; }
        private Timer RocksmithChallengeTimer { get; set; }
        private Timer ChatConnectionTimer { get; set; }

        private int _maxTimerMinutesRocksmith = 165;
        private int _maxTimerMinutesGaming = 120;

        private int _chattyTimerCounter = 0;
        private int _minutesBetweenChattyCommands = 15;

        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly TwitchAPI _twitchApi;

        public ChatbotService(ICommandHelper commandHelper,
            ITwitchClientFactory twitchClientFactory,
            ITwitchApiFactory twitchApiFactory,
        TwitchPubSub pubsub, 
            ITwitchLiveStreamMonitorFactory twitchLiveStreamMonitorFactory,
            IVipApiClient vipApiClient,
            IConfigService configService,
            IStreamStatusApiClient streamStatusApiClient,
            ISecretService secretService,
            ILogger<ChatbotService> logger)
        {
            _commandHelper = commandHelper;
            _twitchClientFactory = twitchClientFactory;
            _twitchApi = twitchApiFactory.Get();
            _pubsub = pubsub;
            _twitchLiveStreamMonitorFactory = twitchLiveStreamMonitorFactory;
            _vipApiClient = vipApiClient;
            _configService = configService;
            _streamStatusApiClient = streamStatusApiClient;
            _secretService = secretService;
            _logger = logger;

            _streamerChannel = _configService.Get<string>("StreamerChannel");
            _isDevelopmentBuild = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" ||
                                  Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Local";

            InitialiseClientEvents();

            _liveStreamMonitor = _twitchLiveStreamMonitorFactory.Get();
            
            _liveStreamMonitor.SetChannelsByName(new List<string>{_streamerChannel});
            _liveStreamMonitor.OnStreamOnline += OnStreamOnline;
            _liveStreamMonitor.OnStreamOffline += OnStreamOffline;
            _liveStreamMonitor.OnServiceStarted += OnStreamMonitorStarted;
            //this.liveStreamMonitor.OnStreamUpdate += OnStreamUpdate;

            _liveStreamMonitor.Start();

            _pubsub.OnPubSubServiceConnected += OnPubSubConnected;
            _pubsub.OnBitsReceived += OnBitsReceived;
            _pubsub.OnListenResponse += OnListenResponse;
            _pubsub.OnChannelSubscription += OnSub;

            _pubsub.Connect();
        }

        private void InitialiseClientEvents()
        {
            _client = _twitchClientFactory.Get();

            _client.OnJoinedChannel += OnJoinedChannel;
            _client.OnChatCommandReceived += OnCommandReceived;
            _client.OnCommunitySubscription += OnSubBomb;
            _client.OnRaidNotification += OnRaidNotification;
            _client.OnDisconnected += OnDisconnected;
            _client.OnError += OnError;
            _client.OnConnectionError += OnConnectionError;
            _client.Connect();
        }

        private void OnConnectionError(object sender, OnConnectionErrorArgs e)
        {
            _logger.LogError($"Error encountered when connected to chat: {e.Error.Message}");
        }

        private void JoinChannel()
        {
            if (_isDevelopmentBuild)
            {
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
                var senderClient = (TwitchClient)sender;
                if (!senderClient.JoinedChannels.Any())
                {
                    senderClient.JoinChannel(_streamerChannel);
                }

                _commandHelper.ProcessCommand(
                    e.Command.CommandText,
                    senderClient,
                    e.Command.ChatMessage.Username,
                    e.Command.ArgumentsAsString,
                    e.Command.ChatMessage.IsModerator || e.Command.ChatMessage.IsBroadcaster,
                    senderClient.JoinedChannels.FirstOrDefault(jc => jc.Channel == e.Command.ChatMessage.Channel));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on processing incoming command");
            }
        }

        private void OnSub(object sender, OnChannelSubscriptionArgs e)
        {
            try
            {
                if (e.Subscription.Context == "subgift")
                {
                    _logger.LogInformation(
                        $"Gifted Sub! - {e.Subscription.RecipientName} - {e.Subscription.Months ?? 1} months, {e.Subscription.StreakMonths} in a row!");

                    _vipApiClient.GiveSubscriptionVips(new GiveSubscriptionVipsRequest
                    {
                        UserSubDetails = new List<UserSubDetail>
                        {
                            new UserSubDetail
                            {
                                Username = e.Subscription.RecipientName,
                                SubscriptionTier = VipHelper.GetSubTier(e),
                                TotalSubMonths = e.Subscription.Months ?? 1
                            }
                        }
                    });

                    // Gifted Sub!
                    OnGiftSub(sender, e);
                }
                else
                {
                    _logger.LogInformation(
                        $"Subscription! - {e.Subscription.Username} - {e.Subscription.CumulativeMonths ?? 1} months, {e.Subscription.StreakMonths} in a row!");

                    _vipApiClient.GiveSubscriptionVips(new GiveSubscriptionVipsRequest
                    {
                        UserSubDetails = new List<UserSubDetail>
                        {
                            new UserSubDetail
                            {
                                Username = e.Subscription.Username,
                                SubscriptionTier = VipHelper.GetSubTier(e),
                                TotalSubMonths = e.Subscription.CumulativeMonths ?? 1
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on Sub", new {e});
            }
        }

        private void OnGiftSub(object sender, OnChannelSubscriptionArgs e)
        {
            try
            {
                _logger.LogInformation($"Gifted Sub! {e.Subscription.RecipientName} has received a sub from {e.Subscription.Username}");

                // Anonymous gift will be blank
                if (!string.IsNullOrWhiteSpace(e.Subscription.Username))
                {
                    // Half as thanks to the gifter
                    _vipApiClient.GiveGiftSubBytes(new GiveGiftSubBytesRequest
                    {
                        Username = e.Subscription.Username
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error OnGiftSub");
            }
        }

        private void OnSubBomb(object sender, OnCommunitySubscriptionArgs e)
        {
            try
            {
                _logger.LogInformation($"Sub Bomb!!! {e.GiftedSubscription.DisplayName} has gifted {e.GiftedSubscription.MsgParamMassGiftCount} subs!");
                
                // Leaving blank for now, it is assumed that subbomb gifts go through gift event too, so relevant vips and bytes will be handled there.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error OnSubBomb");
            }
        }

        private void OnPubSubConnected(object sender, EventArgs e)
        {
            try
            {
                _logger.LogInformation("PubSub Connected!");

                _pubsub.ListenToBitsEventsV2(_configService.Get<string>("ChannelId"));
                _pubsub.ListenToSubscriptions(_configService.Get<string>("ChannelId"));

                _pubsub.SendTopics(_secretService.GetSecret<string>("ChatbotAccessToken"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error OnPubSubConnected");
            }
        }

        private void OnListenResponse(object sender, OnListenResponseArgs e)
        {
            if (e.Successful)
                _logger.LogInformation($"Successfully verified listening to topic: {e.Topic}");
            else 
                _logger.LogError($"OnListenResponse - Failed to listen! {e.Topic} - Error: {e.Response.Error}");
        }

        private void OnBitsReceived(object sender, OnBitsReceivedArgs e)
        {
            try
            {
                _logger.LogInformation(
                    $"Bits Dropped :O!  {e.Username} dropped {e.BitsUsed} - Total {e.TotalBitsUsed}");

                _vipApiClient.UpdateBitsDropped(new UpdateTotalBitsDroppedRequest
                {
                    Username = e.Username,
                    TotalBitsDropped = e.TotalBitsUsed
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error OnBitsReceived");
            }
        }

        private void OnStreamOnline(object sender, OnStreamOnlineArgs e)
        {
            try
            {
                _logger.LogInformation("Streamer is online");

                if (_client.IsConnected && _client.JoinedChannels.Any())
                {
                    _client.SendMessage(e.Channel,
                        $"Looks like @{e.Channel} has come online, better get to work!");
                }
                else
                {
                    _logger.LogInformation("Attempting Reconnect");
                    _twitchClientFactory.Reconnect();

                    InitialiseClientEvents();
                }

                _streamStatusApiClient.SaveStreamStatus(new PutStreamStatusRequest
                {
                    BroadcasterUsername = e.Channel.ToLower(),
                    IsOnline = true
                }).Wait();

                ScheduleStreamTasks(e.Stream.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error OnStreamOnline");
            }
        }

        private void OnStreamOffline(object sender, OnStreamOfflineArgs e)
        {
            try
            {
                _logger.LogInformation("Streamer is offline");

                if (_client.IsConnected && _client.JoinedChannels.Any())
                {
                    _client.SendMessage(e.Channel, $"Looks like @{e.Channel} has gone offline, *yawn* powering down");
                }

                _streamStatusApiClient.SaveStreamStatus(new PutStreamStatusRequest
                {
                    BroadcasterUsername = e.Channel.ToLower(),
                    IsOnline = false
                }).Wait();

                UnScheduleStreamTasks();

                _client.Disconnect();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error OnStreamOffline");
            }
        }

        private void OnStreamMonitorStarted(object sender, OnServiceStartedArgs e)
        {
            _logger.LogInformation($"Stream Monitor Started - Monitoring Channels: {e}");
        }

        private void OnStreamUpdate(object sender, OnStreamUpdateArgs e)
        {
            _logger.LogInformation("Assuming stream category or title has updated, rescheduling tasks");
            UnScheduleStreamTasks();
            //ScheduleStreamTasks(e.Stream.Title);
        }

        private void OnRaidNotification(object sender, OnRaidNotificationArgs e)
        {
            if (int.TryParse(e.RaidNotification.MsgParamViewerCount, out var viewersCount))
            {
                WelcomeRaidOrHost(e.Channel, e.RaidNotification.MsgParamDisplayName, viewersCount, true);
            }
            else
            {
                _logger.LogError($"Raid Notification count is not a number: {e.RaidNotification?.MsgParamViewerCount}");
            }
        }

        private void WelcomeRaidOrHost(string hostedChannelName, string username, int numberofRaiders, bool isRaid)
        {
            var typeText = isRaid ? "raid" : "host";
            _client.SendMessage(hostedChannelName,
                $"Hey everyone, we're getting a {typeText} from @{username} with {numberofRaiders} of their friends! Welcome one and all! codedgUitar");
        }

        private async void ScheduleStreamTasks(string streamGame = "Rocksmith 2014")
        {
            _logger.LogInformation($"Stream tasks scheduled: {streamGame}");
            var isStreamingRocksmith = streamGame.Contains("!request"); // TODO: This needs to query the actual game id as this currently doesn't work correctly
            var maxTimerMinutes =
                TimeSpan.FromMinutes(isStreamingRocksmith ? _maxTimerMinutesRocksmith : _maxTimerMinutesGaming);

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
            //RocksmithChallengeTimer = new Timer(
            //    e => _commandHelper.ProcessCommand("challenge", _client, "Chatbot", string.Empty, true, joinedRoom),
            //    null,
            //    AssignChattyTimer(), maxTimerMinutes);
            //MerchTimer = new Timer(
            //    e => _commandHelper.ProcessCommand("merch", _client, "Chatbot", string.Empty, true, joinedRoom),
            //    null,
            //    AssignChattyTimer(), maxTimerMinutes);

            ChatConnectionTimer = new Timer(
                async e =>
                {
                    try
                    {
                        var authToken = await _twitchApi.Helix.Users.GetUsersAsync();
                        var chattersResponse = await _twitchApi.Helix.Chat.GetChattersAsync(authToken.Users.FirstOrDefault()?.Id, authToken.Users.FirstOrDefault()?.Id, 1000);

                        if (chattersResponse.Data.Any())
                        {
                            var chatters = chattersResponse.Data.Select(d => d.UserLogin);
                            if (chatters.Contains(_configService.Get<string>("ChatbotNick"))) return;

                            _logger.LogError($"DISCONNECTED FROM CHAT, RECONNECTING");

                            _twitchClientFactory.Reconnect();
                            InitialiseClientEvents();
                        }
                        else _logger.LogError("Could not retrieve Chatters JSON from TMI service in ChatConnectionTimer");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Could not check tmi service inside ChatConnectionTimer");
                    }
                },
                null,
                TimeSpan.FromSeconds(600),
                TimeSpan.FromSeconds(120));
        }
        
        private void OnError(object sender, OnErrorEventArgs e)
        {
            _logger.LogError(e.Exception, $"Error encountered in Client.OnError method");
        }

        private void OnDisconnected(object sender, OnDisconnectedEventArgs e)
        {
            _logger.LogError("DISCONNECTED FROM CHAT");
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
            //MerchTimer?.Dispose();
            LegatorTimer?.Dispose();
            ShredFestTimer?.Dispose();
            RocksmithChallengeTimer?.Dispose();
            BytesTimer?.Dispose();
            DonationsTimer?.Dispose();
            ChatConnectionTimer?.Dispose();
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
