using System.Threading.Tasks;
using CoreCodedChatbot.ApiContract.SharedExternalRequestModels;

namespace CoreCodedChatbot.Interfaces
{
    public interface IRedditHelper
    {
        Task<RedditBasicInfo> GetRandomPost(string subreddit);
    }
}
