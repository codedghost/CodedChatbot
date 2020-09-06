﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Enums;
using TwitchLib.Communication.Events;
using TwitchLib.Communication.Models;
using TwitchLib.PubSub.Enums;
using TwitchLib.PubSub.Events;
using TwitchLib.PubSub.Interfaces;
using TwitchLib.PubSub.Models;
using TwitchLib.PubSub.Models.Responses.Messages;

namespace TwitchLib.PubSub
{
    /// <summary>
    /// Class represneting interactions with the Twitch PubSub
    /// Implements the <see cref="ITwitchPubSub" />
    /// </summary>
    /// <seealso cref="ITwitchPubSub" />
    public class TwitchPubSub : ITwitchPubSub
    {
        /// <summary>
        /// The socket
        /// </summary>
        private readonly WebSocketClient _socket;
        /// <summary>
        /// The previous requests
        /// </summary>
        private readonly List<PreviousRequest> _previousRequests = new List<PreviousRequest>();
        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger<TwitchPubSub> _logger;
        /// <summary>
        /// The ping timer
        /// </summary>
        private readonly Timer _pingTimer = new Timer();
        /// <summary>
        /// The topic list
        /// </summary>
        private readonly List<string> _topicList = new List<string>();

        private readonly Dictionary<string, string> _topicToChannelId = new Dictionary<string, string>();

        #region Events
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub Service is connected.
        /// </summary>
        public event EventHandler OnPubSubServiceConnected;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub Service has an error.
        /// </summary>
        public event EventHandler<OnPubSubServiceErrorArgs> OnPubSubServiceError;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub Service is closed.
        /// </summary>
        public event EventHandler OnPubSubServiceClosed;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives any response.
        /// </summary>
        public event EventHandler<OnListenResponseArgs> OnListenResponse;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice a viewer gets a timeout.
        /// </summary>
        public event EventHandler<OnTimeoutArgs> OnTimeout;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice a viewer gets banned.
        /// </summary>
        public event EventHandler<OnBanArgs> OnBan;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice a message was deleted.
        /// </summary>
        public event EventHandler<OnMessageDeletedArgs> OnMessageDeleted;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice a viewer gets unbanned.
        /// </summary>
        public event EventHandler<OnUnbanArgs> OnUnban;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice a viewer gets a timeout removed.
        /// </summary>
        public event EventHandler<OnUntimeoutArgs> OnUntimeout;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice that the channel being listened to is hosting another channel.
        /// </summary>
        public event EventHandler<OnHostArgs> OnHost;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice that Sub-Only Mode gets turned on.
        /// </summary>
        public event EventHandler<OnSubscribersOnlyArgs> OnSubscribersOnly;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice that Sub-Only Mode gets turned off.
        /// </summary>
        public event EventHandler<OnSubscribersOnlyOffArgs> OnSubscribersOnlyOff;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice that chat gets cleared.
        /// </summary>
        public event EventHandler<OnClearArgs> OnClear;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice that Emote-Only Mode gets turned on.
        /// </summary>
        public event EventHandler<OnEmoteOnlyArgs> OnEmoteOnly;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice that Emote-Only Mode gets turned off.
        /// </summary>
        public event EventHandler<OnEmoteOnlyOffArgs> OnEmoteOnlyOff;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice that the chat option R9kBeta gets turned on.
        /// </summary>
        public event EventHandler<OnR9kBetaArgs> OnR9kBeta;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice that the chat option R9kBeta gets turned off.
        /// </summary>
        public event EventHandler<OnR9kBetaOffArgs> OnR9kBetaOff;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice of a bit donation.
        /// </summary>
        public event EventHandler<OnBitsReceivedArgs> OnBitsReceived;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice of a commerce transaction.
        /// </summary>
        public event EventHandler<OnChannelCommerceReceivedArgs> OnChannelCommerceReceived;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice that the stream of the channel being listened to goes online.
        /// </summary>
        public event EventHandler<OnStreamUpArgs> OnStreamUp;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice that the stream of the channel being listened to goes offline.
        /// </summary>
        public event EventHandler<OnStreamDownArgs> OnStreamDown;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice view count has changed.
        /// </summary>
        public event EventHandler<OnViewCountArgs> OnViewCount;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives a whisper.
        /// </summary>
        public event EventHandler<OnWhisperArgs> OnWhisper;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice when the channel being listened to gets a subscription.
        /// </summary>
        public event EventHandler<OnChannelSubscriptionArgs> OnChannelSubscription;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives a message sent to the specified extension on the specified channel.
        /// </summary>
        public event EventHandler<OnChannelExtensionBroadcastArgs> OnChannelExtensionBroadcast;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice when a user follows the designated channel.
        /// </summary>
        public event EventHandler<OnFollowArgs> OnFollow;
        /// <inheritdoc />
        /// <summary>
        /// Fires when pubsub receives notice when a custom reward has been created on the specified channel.
        ///</summary>
        public event EventHandler<OnCustomRewardCreatedArgs> OnCustomRewardCreated;
        /// <inheritdoc />
        /// <summary>
        /// Fires when pubsub receives notice when a custom reward has been changed on the specified channel.
        ///</summary>
        public event EventHandler<OnCustomRewardUpdatedArgs> OnCustomRewardUpdated;

        /// <inheritdoc />
        /// <summary>
        /// Fires when pubsub receives notice when a reward has been deleted on the specified channel.</summary>
        /// </summary>
        public event EventHandler<OnCustomRewardDeletedArgs> OnCustomRewardDeleted;
        /// <inheritdoc />
        /// <summary>
        /// Fires when pubsub receives notice when a reward has been redeemed on the specified channel.</summary>
        /// </summary>
        public event EventHandler<OnRewardRedeemedArgs> OnRewardRedeemed;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice when the leaderboard changes for subs.
        /// </summary>
        public event EventHandler<OnLeaderboardEventArgs> OnLeaderboardSubs;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice when the leaderboard changes for Bits.
        /// </summary>
        public event EventHandler<OnLeaderboardEventArgs> OnLeaderboardBits;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice when a channel prepares a raid
        /// </summary>
        public event EventHandler<OnRaidUpdateArgs> OnRaidUpdate;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice when a channel prepares a raid
        /// </summary>
        public event EventHandler<OnRaidUpdateV2Args> OnRaidUpdateV2;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives notice when a channel starts the raid
        /// </summary>
        public event EventHandler<OnRaidGoArgs> OnRaidGo;
        /// <inheritdoc />
        /// <summary>
        /// Fires when PubSub receives any data from Twitch
        /// </summary>
        public event EventHandler<OnLogArgs> OnLog;
        /// <inheritdoc/>
        /// <summary>
        /// Fires when PubSub receives notice that the stream is playing a commercial.
        /// </summary>
        public event EventHandler<OnCommercialArgs> OnCommercial;
        #endregion

        /// <summary>
        /// Constructor for a client that interface's with Twitch's PubSub system.
        /// </summary>
        /// <param name="logger">Optional ILogger param to enable logging</param>
        public TwitchPubSub(ILogger<TwitchPubSub> logger = null)
        {
            _logger = logger;

            var options = new ClientOptions { ClientType = ClientType.PubSub };
            _socket = new WebSocketClient(options);

            _socket.OnConnected += Socket_OnConnected;
            _socket.OnError += OnError;
            _socket.OnMessage += OnMessage; ;
            _socket.OnDisconnected += Socket_OnDisconnected;
        }

        /// <summary>
        /// Handles the <see cref="E:OnError" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="OnErrorEventArgs"/> instance containing the event data.</param>
        private void OnError(object sender, OnErrorEventArgs e)
        {
            _logger?.LogError($"OnError in PubSub Websocket connection occured! Exception: {e.Exception}");
            OnPubSubServiceError?.Invoke(this, new OnPubSubServiceErrorArgs { Exception = e.Exception });
        }

        /// <summary>
        /// Handles the <see cref="E:OnMessage" /> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="OnMessageEventArgs"/> instance containing the event data.</param>
        private void OnMessage(object sender, OnMessageEventArgs e)
        {
            _logger?.LogDebug($"Received Websocket OnMessage: {e.Message}");
            OnLog?.Invoke(this, new OnLogArgs { Data = e.Message });
            ParseMessage(e.Message);
        }

        /// <summary>
        /// Handles the OnDisconnected event of the Socket control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Socket_OnDisconnected(object sender, EventArgs e)
        {
            _logger?.LogWarning("PubSub Websocket connection closed");
            _pingTimer.Stop();
            OnPubSubServiceClosed?.Invoke(this, null);
        }

        /// <summary>
        /// Handles the OnConnected event of the Socket control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Socket_OnConnected(object sender, EventArgs e)
        {
            _logger?.LogInformation("PubSub Websocket connection established");
            _pingTimer.Interval = 180000;
            _pingTimer.Elapsed += PingTimerTick;
            _pingTimer.Start();
            OnPubSubServiceConnected?.Invoke(this, null);
        }

        /// <summary>
        /// Pings the timer tick.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="ElapsedEventArgs"/> instance containing the event data.</param>
        private void PingTimerTick(object sender, ElapsedEventArgs e)
        {
            var data = new JObject(
                new JProperty("type", "PING")
            );
            _socket.Send(data.ToString());
        }

        /// <summary>
        /// Parses the message.
        /// </summary>
        /// <param name="message">The message.</param>
        private void ParseMessage(string message)
        {
            var type = JObject.Parse(message).SelectToken("type")?.ToString();

            switch (type?.ToLower())
            {
                case "response":
                    var resp = new Models.Responses.Response(message);
                    if (_previousRequests.Count != 0)
                    {
                        bool handled = false;
                        foreach (var request in _previousRequests)
                        {
                            if (string.Equals(request.Nonce, resp.Nonce, StringComparison.CurrentCulture))
                            {
                                OnListenResponse?.Invoke(this, new OnListenResponseArgs { Response = resp, Topic = request.Topic, Successful = resp.Successful });
                                handled = true;
                            }
                        }
                        if (handled) return;
                    }
                    break;
                case "message":
                    var msg = new Models.Responses.Message(message);
                    _topicToChannelId.TryGetValue(msg.Topic, out var channelId);
                    channelId = channelId ?? "";
                    switch (msg.Topic.Split('.')[0])
                    {
                        case "channel-subscribe-events-v1":
                            var subscription = msg.MessageData as ChannelSubscription;
                            OnChannelSubscription?.Invoke(this, new OnChannelSubscriptionArgs { Subscription = subscription, ChannelId = channelId });
                            return;
                        case "whispers":
                            var whisper = (Whisper)msg.MessageData;
                            OnWhisper?.Invoke(this, new OnWhisperArgs { Whisper = whisper, ChannelId = channelId });
                            return;
                        case "chat_moderator_actions":
                            var cma = msg.MessageData as ChatModeratorActions;
                            var reason = "";
                            var targetChannelId = msg.Topic.Split('.')[2];
                            switch (cma?.ModerationAction.ToLower())
                            {
                                case "timeout":
                                    if (cma.Args.Count > 2)
                                        reason = cma.Args[2];
                                    OnTimeout?.Invoke(this, new OnTimeoutArgs
                                    {
                                        TimedoutBy = cma.CreatedBy,
                                        TimedoutById = cma.CreatedByUserId,
                                        TimedoutUserId = cma.TargetUserId,
                                        TimeoutDuration = TimeSpan.FromSeconds(int.Parse(cma.Args[1])),
                                        TimeoutReason = reason,
                                        TimedoutUser = cma.Args[0],
                                        ChannelId = channelId
                                    });
                                    return;
                                case "ban":
                                    if (cma.Args.Count > 1)
                                        reason = cma.Args[1];
                                    OnBan?.Invoke(this, new OnBanArgs { BannedBy = cma.CreatedBy, BannedByUserId = cma.CreatedByUserId, BannedUserId = cma.TargetUserId, BanReason = reason, BannedUser = cma.Args[0], ChannelId = channelId });
                                    return;
                                case "delete":
                                    OnMessageDeleted?.Invoke(this, new OnMessageDeletedArgs { DeletedBy = cma.CreatedBy, DeletedByUserId = cma.CreatedByUserId, TargetUserId = cma.TargetUserId, TargetUser = cma.Args[0], Message = cma.Args[1], MessageId = cma.Args[2], ChannelId = channelId });
                                    return;
                                case "unban":
                                    OnUnban?.Invoke(this, new OnUnbanArgs { UnbannedBy = cma.CreatedBy, UnbannedByUserId = cma.CreatedByUserId, UnbannedUserId = cma.TargetUserId, UnbannedUser = cma.Args[0], ChannelId = channelId });
                                    return;
                                case "untimeout":
                                    OnUntimeout?.Invoke(this, new OnUntimeoutArgs { UntimeoutedBy = cma.CreatedBy, UntimeoutedByUserId = cma.CreatedByUserId, UntimeoutedUserId = cma.TargetUserId, UntimeoutedUser = cma.Args[0], ChannelId = channelId });
                                    return;
                                case "host":
                                    OnHost?.Invoke(this, new OnHostArgs { HostedChannel = cma.Args[0], Moderator = cma.CreatedBy, ChannelId = channelId });
                                    return;
                                case "subscribers":
                                    OnSubscribersOnly?.Invoke(this, new OnSubscribersOnlyArgs { Moderator = cma.CreatedBy, ChannelId = channelId });
                                    return;
                                case "subscribersoff":
                                    OnSubscribersOnlyOff?.Invoke(this, new OnSubscribersOnlyOffArgs { Moderator = cma.CreatedBy, ChannelId = channelId });
                                    return;
                                case "clear":
                                    OnClear?.Invoke(this, new OnClearArgs { Moderator = cma.CreatedBy, ChannelId = channelId });
                                    return;
                                case "emoteonly":
                                    OnEmoteOnly?.Invoke(this, new OnEmoteOnlyArgs { Moderator = cma.CreatedBy, ChannelId = channelId });
                                    return;
                                case "emoteonlyoff":
                                    OnEmoteOnlyOff?.Invoke(this, new OnEmoteOnlyOffArgs { Moderator = cma.CreatedBy, ChannelId = channelId });
                                    return;
                                case "r9kbeta":
                                    OnR9kBeta?.Invoke(this, new OnR9kBetaArgs { Moderator = cma.CreatedBy, ChannelId = channelId });
                                    return;
                                case "r9kbetaoff":
                                    OnR9kBetaOff?.Invoke(this, new OnR9kBetaOffArgs { Moderator = cma.CreatedBy, ChannelId = channelId });
                                    return;
                            }
                            break;
                        case "channel-bits-events-v1":
                            if (msg.MessageData is ChannelBitsEvents cbe)
                            {
                                OnBitsReceived?.Invoke(this, new OnBitsReceivedArgs
                                {
                                    BitsUsed = cbe.BitsUsed,
                                    ChannelId = cbe.ChannelId,
                                    ChannelName = cbe.ChannelName,
                                    ChatMessage = cbe.ChatMessage,
                                    Context = cbe.Context,
                                    Time = cbe.Time,
                                    TotalBitsUsed = cbe.TotalBitsUsed,
                                    UserId = cbe.UserId,
                                    Username = cbe.Username
                                });
                                return;
                            }
                            break;
                        case "channel-commerce-events-v1":
                            if (msg.MessageData is ChannelCommerceEvents cce)
                            {
                                OnChannelCommerceReceived?.Invoke(this, new OnChannelCommerceReceivedArgs
                                {

                                    Username = cce.Username,
                                    DisplayName = cce.DisplayName,
                                    ChannelName = cce.ChannelName,
                                    UserId = cce.UserId,
                                    ChannelId = cce.ChannelId,
                                    Time = cce.Time,
                                    ItemImageURL = cce.ItemImageURL,
                                    ItemDescription = cce.ItemDescription,
                                    SupportsChannel = cce.SupportsChannel,
                                    PurchaseMessage = cce.PurchaseMessage
                                });
                                return;
                            }
                            break;
                        case "channel-ext-v1":
                            var cEB = msg.MessageData as ChannelExtensionBroadcast;
                            OnChannelExtensionBroadcast?.Invoke(this, new OnChannelExtensionBroadcastArgs { Messages = cEB.Messages, ChannelId = channelId });
                            return;
                        case "video-playback":
                            var vP = msg.MessageData as VideoPlayback;
                            switch (vP?.Type)
                            {
                                case VideoPlaybackType.StreamDown:
                                    OnStreamDown?.Invoke(this, new OnStreamDownArgs { ServerTime = vP.ServerTime });
                                    return;
                                case VideoPlaybackType.StreamUp:
                                    OnStreamUp?.Invoke(this, new OnStreamUpArgs { PlayDelay = vP.PlayDelay, ServerTime = vP.ServerTime });
                                    return;
                                case VideoPlaybackType.ViewCount:
                                    OnViewCount?.Invoke(this, new OnViewCountArgs { ServerTime = vP.ServerTime, Viewers = vP.Viewers });
                                    return;
                                case VideoPlaybackType.Commercial:
                                    OnCommercial?.Invoke(this, new OnCommercialArgs { ServerTime = vP.ServerTime, Length = vP.Length });
                                    return;
                            }
                            break;
                        case "following":
                            var f = (Following) msg.MessageData;
                            f.FollowedChannelId = msg.Topic.Split('.')[1];
                            OnFollow?.Invoke(this, new OnFollowArgs { FollowedChannelId = f.FollowedChannelId, DisplayName = f.DisplayName, UserId = f.UserId, Username = f.Username });
                            return;
                        case "community-points-channel-v1":
                            var cpc = msg.MessageData as CommunityPointsChannel;
                            switch (cpc?.Type)
                            {
                                case CommunityPointsChannelType.RewardRedeemed:
                                    OnRewardRedeemed?.Invoke(this, new OnRewardRedeemedArgs { TimeStamp = cpc.TimeStamp, ChannelId = cpc.ChannelId, Login = cpc.Login, DisplayName = cpc.DisplayName, Message = cpc.Message, RewardId = cpc.RewardId, RewardTitle = cpc.RewardTitle, RewardPrompt = cpc.RewardPrompt, RewardCost = cpc.RewardCost, Status = cpc.Status });
                                    return;
                                case CommunityPointsChannelType.CustomRewardUpdated:
                                    OnCustomRewardUpdated?.Invoke(this, new OnCustomRewardUpdatedArgs { TimeStamp = cpc.TimeStamp, ChannelId =  cpc.ChannelId, RewardId = cpc.RewardId, RewardTitle = cpc.RewardTitle, RewardPrompt = cpc.RewardPrompt, RewardCost = cpc.RewardCost });
                                    return;
                                case CommunityPointsChannelType.CustomRewardCreated:
                                    OnCustomRewardCreated?.Invoke(this, new OnCustomRewardCreatedArgs { TimeStamp = cpc.TimeStamp, ChannelId = cpc.ChannelId, RewardId = cpc.RewardId, RewardTitle = cpc.RewardTitle, RewardPrompt = cpc.RewardPrompt, RewardCost = cpc.RewardCost });
                                    return;
                                case CommunityPointsChannelType.CustomRewardDeleted:
                                    OnCustomRewardDeleted?.Invoke(this, new OnCustomRewardDeletedArgs { TimeStamp = cpc.TimeStamp, ChannelId = cpc.ChannelId, RewardId = cpc.RewardId, RewardTitle = cpc.RewardTitle, RewardPrompt = cpc.RewardPrompt });
                                    return;
                            }
                            return;
                        case "leaderboard-events-v1":
                            var lbe = msg.MessageData as LeaderboardEvents;
                            switch (lbe?.Type)
                            {
                                case LeaderBoardType.BitsUsageByChannel:
                                    OnLeaderboardBits?.Invoke(this, new OnLeaderboardEventArgs { ChannelId = lbe.ChannelId, TopList = lbe.Top });
                                    return;
                                case LeaderBoardType.SubGiftSent:
                                    OnLeaderboardSubs?.Invoke(this, new OnLeaderboardEventArgs { ChannelId = lbe.ChannelId, TopList = lbe.Top });
                                    return;
                            }
                            return;
                        case "raid":
                            var r = msg.MessageData as RaidEvents;
                            switch (r?.Type)
                            {
                                case RaidType.RaidUpdate:
                                    OnRaidUpdate?.Invoke(this, new OnRaidUpdateArgs{ Id = r.Id, ChannelId = r.ChannelId , TargetChannelId = r.TargetChannelId, AnnounceTime = r.AnnounceTime, RaidTime = r.RaidTime, RemainingDurationSeconds = r.RemainigDurationSeconds, ViewerCount = r.ViewerCount });
                                    return;
                                case RaidType.RaidUpdateV2:
                                    OnRaidUpdateV2?.Invoke(this, new OnRaidUpdateV2Args{ Id = r.Id, ChannelId = r.ChannelId, TargetChannelId = r.TargetChannelId, TargetLogin = r.TargetLogin, TargetDisplayName = r.TargetDisplayName, TargetProfileImage = r.TargetProfileImage, ViewerCount = r.ViewerCount });
                                    return;
                                case RaidType.RaidGo:
                                    OnRaidGo?.Invoke(this, new OnRaidGoArgs { Id = r.Id, ChannelId = r.ChannelId, TargetChannelId = r.TargetChannelId, TargetLogin = r.TargetLogin, TargetDisplayName = r.TargetDisplayName, TargetProfileImage = r.TargetProfileImage, ViewerCount = r.ViewerCount });
                                    return;
                            }
                            return;
                    }
                    break;
            }
            UnaccountedFor(message);
        }

        /// <summary>
        /// The random
        /// </summary>
        private static readonly Random Random = new Random();
        /// <summary>
        /// Generates the nonce.
        /// </summary>
        /// <returns>System.String.</returns>
        private static string GenerateNonce()
        {
            return new string(Enumerable.Repeat("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 8)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Listens to topic.
        /// </summary>
        /// <param name="topic">The topic.</param>
        private void ListenToTopic(string topic)
        {
            _topicList.Add(topic);
        }

        /// <summary>
        /// Listen to multiple topics.
        /// </summary>
        /// <param name="topics">The topics</param>
        private void ListenToTopics(params string[] topics)
        {
            foreach (var topic in topics)
            {
                _topicList.Add(topic);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Sends the topics.
        /// </summary>
        /// <param name="oauth">The oauth.</param>
        /// <param name="unlisten">if set to <c>true</c> [unlisten].</param>
        public void SendTopics(string oauth = null, bool unlisten = false)
        {
            if (oauth != null && oauth.Contains("oauth:"))
            {
                oauth = oauth.Replace("oauth:", "");
            }

            var nonce = GenerateNonce();

            var topics = new JArray();
            foreach (var val in _topicList)
            {
                _previousRequests.Add(new PreviousRequest(nonce, PubSubRequestType.ListenToTopic, val));
                topics.Add(new JValue(val));
            }

            var jsonData = new JObject(
                new JProperty("type", !unlisten ? "LISTEN" : "UNLISTEN"),
                new JProperty("nonce", nonce),
                new JProperty("data",
                    new JObject(
                        new JProperty("topics", topics)
                        )
                    )
                );
            if (oauth != null)
            {
                ((JObject)jsonData.SelectToken("data")).Add(new JProperty("auth_token", oauth));
            }

            _socket.Send(jsonData.ToString());

            _topicList.Clear();
        }

        /// <summary>
        /// Unaccounteds for.
        /// </summary>
        /// <param name="message">The message.</param>
        private void UnaccountedFor(string message)
        {
            _logger?.LogInformation($"[TwitchPubSub] {message}");
        }

        #region Listeners
        /// <inheritdoc />
        /// <summary>
        /// Sends a request to listenOn follows coming into a specified channel.
        /// </summary>
        /// <param name="channelId">The channel identifier.</param>
        public void ListenToFollows(string channelId)
        {
            var topic = $"following.{channelId}";
            _topicToChannelId[topic] = channelId;
            ListenToTopic(topic);
        }

        /// <inheritdoc />
        /// <summary>
        /// Sends a request to listenOn timeouts and bans in a specific channel
        /// </summary>
        /// <param name="myTwitchId">A moderator's twitch acount's ID (can be fetched from TwitchApi)</param>
        /// <param name="channelTwitchId">Channel ID who has previous parameter's moderator (can be fetched from TwitchApi)</param>
        public void ListenToChatModeratorActions(string myTwitchId, string channelTwitchId)
        {
            var topic = $"chat_moderator_actions.{myTwitchId}.{channelTwitchId}";
            _topicToChannelId[topic] = channelTwitchId;
            ListenToTopic(topic);
        }

        /// <inheritdoc />
        /// <summary>
        /// Sends a request to ListenOn EBS broadcasts sent to a specific extension on a specific channel.
        /// </summary>
        /// <param name="channelId">Id of the channel that the extension lives on.</param>
        /// <param name="extensionId">The extension identifier.</param>
        public void ListenToChannelExtensionBroadcast(string channelId, string extensionId)
        {
            var topic = $"channel-ext-v1.{channelId}-{extensionId}-broadcast";
            _topicToChannelId[topic] = channelId;
            ListenToTopic(topic);
        }

        /// <inheritdoc />
        /// <summary>
        /// Sends request to listenOn bits events in specific channel
        /// </summary>
        /// <param name="channelTwitchId">Channel Id of channel to listen to bits on (can be fetched from TwitchApi)</param>
        public void ListenToBitsEvents(string channelTwitchId)
        {
            var topic = $"channel-bits-events-v1.{channelTwitchId}";
            _topicToChannelId[topic] = channelTwitchId;
            ListenToTopic(topic);
        }

        /// <inheritdoc />
        /// <summary>
        /// Sends request to listenOn channel commerce events in specific channel
        /// </summary>
        /// <param name="channelTwitchId">Channel Id of channel to listen to commerce events on.</param>
        public void ListenToCommerce(string channelTwitchId)
        {
            var topic = $"channel-commerce-events-v1.{channelTwitchId}";
            _topicToChannelId[topic] = channelTwitchId;
            ListenToTopic(topic);
        }

        /// <inheritdoc />
        /// <summary>
        /// Sends request to listenOn video playback events in specific channel
        /// </summary>
        /// <param name="channelName">Name of channel to listen to playback events in.</param>
        public void ListenToVideoPlayback(string channelName)
        {
            ListenToTopic($"video-playback.{channelName}");
        }

        /// <inheritdoc />
        /// <summary>
        /// Sends request to listen to whispers from specific channel.
        /// </summary>
        /// <param name="channelTwitchId">Channel to listen to whispers on.</param>
        public void ListenToWhispers(string channelTwitchId)
        {
            var topic = $"whispers.{channelTwitchId}";
            _topicToChannelId[topic] = channelTwitchId;
            ListenToTopic(topic);
        }

        /// <inheritdoc />
        /// <summary>
        /// Sends request to listen to rewards from specific channel.
        /// </summary>
        /// <param name="channelTwitchId">Channel to listen to rewards on.</param>
        public void ListenToRewards(string channelTwitchId)
        {
            var topic = $"community-points-channel-v1.{channelTwitchId}";
            _topicToChannelId[topic] = channelTwitchId;
            ListenToTopic(topic);
        }

        /// <inheritdoc />
        /// <summary>
        /// Sends request to listen to leaderboards from specific channel.
        /// </summary>
        /// <param name="channelTwitchId">Channel to listen to leaderboards on.</param>
        public void ListenToLeaderboards(string channelTwitchId)
        {
            var topicBits = $"leaderboard-events-v1.bits-usage-by-channel-v1-{channelTwitchId}-WEEK";
            var topicSubs = $"leaderboard-events-v1.sub-gift-sent-{channelTwitchId}-WEEK";
            _topicToChannelId[topicBits] = channelTwitchId;
            _topicToChannelId[topicSubs] = channelTwitchId;
            ListenToTopics(topicBits, topicSubs);
        }

        /// <inheritdoc />
        /// <summary>
        /// Sends request to listen to raids 'from' specific channel
        /// </summary>
        /// <param name="channelTwitchId">Channel to listen to raids get prepared on.</param>
        public void ListenToRaid(string channelTwitchId)
        {
            var topicRaid = $"raid.{channelTwitchId}";
            _topicToChannelId[topicRaid] = channelTwitchId;
            ListenToTopic(topicRaid);
        }

        /// <inheritdoc />
        /// <summary>
        /// Sends request to listen to channel subscriptions.
        /// </summary>
        /// <param name="channelId">Id of the channel to listen to.</param>
        public void ListenToSubscriptions(string channelId)
        {
            var topic = $"channel-subscribe-events-v1.{channelId}";
            _topicToChannelId[topic] = channelId;
            ListenToTopic(topic);
        }
        #endregion

        /// <inheritdoc />
        /// <summary>
        /// Method to connect to Twitch's PubSub service. You MUST listen toOnConnected event and listen to a Topic within 15 seconds of connecting (or be disconnected)
        /// </summary>
        public void Connect()
        {
            _socket.Open();
        }

        /// <inheritdoc />
        /// <summary>
        /// What do you think it does? :)
        /// </summary>
        public void Disconnect()
        {
            _socket.Close();
        }

        /// <inheritdoc />
        /// <summary>
        /// This method will send passed json text to the message parser in order to allow forOn-demand parser testing.
        /// </summary>
        /// <param name="testJsonString">The test json string.</param>
        public void TestMessageParser(string testJsonString)
        {
            ParseMessage(testJsonString);
        }
    }
}
