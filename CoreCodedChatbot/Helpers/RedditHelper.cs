using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Models.Data;
using Newtonsoft.Json;

namespace CoreCodedChatbot.Helpers
{
    public class RedditHelper : IRedditHelper
    {
        public async Task<RedditBasicInfo> GetRandomPost(string subreddit)
        {
            var redditClient = new HttpClient
            {
                BaseAddress = new Uri($"http://www.reddit.com/r/{subreddit}/hot/")
            };

            var rand = new Random();

            var randomPostResponse = await redditClient.GetAsync(".json?limit=100");
            if (!randomPostResponse.IsSuccessStatusCode)
            {
                return null;
            }

            // Ignore self posts, this should ideally leave us with images and video posts
            var topPosts =
                JsonConvert.DeserializeObject<RedditRandomJsonModel>(
                        await randomPostResponse.Content.ReadAsStringAsync()).data.children
                    .Where(ch => !ch.data.domain.Contains($"self.{subreddit}")).ToList();


            // Get Random post
            var randomPost = topPosts[rand.Next(topPosts.Count)];

            return new RedditBasicInfo
            {
                Title = randomPost.data.title,
                Url = $"https://www.reddit.com{randomPost.data.permalink}"
            };
        }
    }
}
