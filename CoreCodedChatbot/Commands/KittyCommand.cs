using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;
using Newtonsoft.Json;
using Serilog.Debugging;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "kitty" }, false)]
    public class KittyCommand : ICommand
    {
        private ConfigModel Config;
        private HttpClient RedditCatClient;
        private Random Rand;

        public KittyCommand(ConfigModel config)
        {
            this.Config = config;

            this.RedditCatClient = new HttpClient
            {
                BaseAddress = new Uri("http://www.reddit.com/r/cats/hot/")
            };

            Rand = new Random();
        }

        public async void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            var randomPostResponse = await RedditCatClient.GetAsync(".json?limit=100");
            if (!randomPostResponse.IsSuccessStatusCode)
            {
                client.SendMessage(joinedChannel,
                    $"Hey @{username}, I can't seem to talk to Reddit right now, try again in a few minutes :(");
            }

            // Ignore self posts, this should ideally leave us with images and video posts
            var topCatPosts =
                JsonConvert.DeserializeObject<RedditRandomJsonModel>(
                        await randomPostResponse.Content.ReadAsStringAsync()).data.children
                    .Where(ch => !ch.data.domain.Contains("self.cats")).ToList();

            // Get Random post
            var randomPost = topCatPosts[Rand.Next(topCatPosts.Count)];

            // Put post link in chat (should attribute reddit and poster rather than posting media directly)
            client.SendMessage(joinedChannel,
                $"Hey @{username}, {randomPost.data.title} - https://www.reddit.com{randomPost.data.permalink}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, This command will give you a picture of a friendly feline!");
        }
    }
}
