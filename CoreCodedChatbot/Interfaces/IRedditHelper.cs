using System.Threading.Tasks;
using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Interfaces
{
    public interface IRedditHelper
    {
        Task<RedditBasicInfo> GetRandomPost(string subreddit);
    }
}
