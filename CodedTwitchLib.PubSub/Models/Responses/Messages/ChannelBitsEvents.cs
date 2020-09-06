﻿using Newtonsoft.Json.Linq;

namespace TwitchLib.PubSub.Models.Responses.Messages
{
    /// <summary>
    /// Model representing the data in a channel bits event.
    /// Implements the <see cref="MessageData" />
    /// </summary>
    /// <seealso cref="MessageData" />
    /// <inheritdoc />
    public class ChannelBitsEvents : MessageData
    {
        /// <summary>
        /// Username of the sender.
        /// </summary>
        /// <value>The username.</value>
        public string Username { get; }
        /// <summary>
        /// The channel the bits were sent to.
        /// </summary>
        /// <value>The name of the channel.</value>
        public string ChannelName { get; }
        /// <summary>
        /// User ID of the sender.
        /// </summary>
        /// <value>The user identifier.</value>
        public string UserId { get; }
        /// <summary>
        /// Channel/User ID of where the bits were sent to.
        /// </summary>
        /// <value>The channel identifier.</value>
        public string ChannelId { get; }
        /// <summary>
        /// Time stamp of the event.
        /// </summary>
        /// <value>The time.</value>
        public string Time { get; }
        /// <summary>
        /// Chat message that accompanied the bits.
        /// </summary>
        /// <value>The chat message.</value>
        public string ChatMessage { get; }
        /// <summary>
        /// The amount of bits sent.
        /// </summary>
        /// <value>The bits used.</value>
        public int BitsUsed { get; }
        /// <summary>
        /// The total amount of bits the user has sent.
        /// </summary>
        /// <value>The total bits used.</value>
        public int TotalBitsUsed { get; }
        /// <summary>
        /// Context related to event.
        /// </summary>
        /// <value>The context.</value>
        public string Context { get; }

        /// <summary>
        /// ChannelBitsEvent model constructor.
        /// </summary>
        /// <param name="jsonStr">The json string.</param>
        public ChannelBitsEvents(string jsonStr)
        {
            var json = JObject.Parse(jsonStr);
            Username = json.SelectToken("data").SelectToken("user_name")?.ToString();
            ChannelName = json.SelectToken("data").SelectToken("channel_name")?.ToString();
            UserId = json.SelectToken("data").SelectToken("user_id")?.ToString();
            ChannelId = json.SelectToken("data").SelectToken("channel_id")?.ToString();
            Time = json.SelectToken("data").SelectToken("time")?.ToString();
            ChatMessage = json.SelectToken("data").SelectToken("chat_message")?.ToString();
            BitsUsed = int.Parse(json.SelectToken("data").SelectToken("bits_used").ToString());
            TotalBitsUsed = int.Parse(json.SelectToken("data").SelectToken("total_bits_used").ToString());
            Context = json.SelectToken("data").SelectToken("context")?.ToString();
        }
    }
}
