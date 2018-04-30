using System.Collections.Generic;
using System.Linq;

using CoreCodedChatbot.Database.Context.Models;

namespace CoreCodedChatbot.Extensions
{
    public static class IQueryableHelper
    {
        public static List<SongRequest> OrderRequests(this IQueryable<SongRequest> requests, bool isCurrentVip)
        {
            var vips = requests.Where(sr => sr.VipRequestTime != null).OrderBy(sr => sr.VipRequestTime);
            var regulars = requests.Where(sr => sr.VipRequestTime == null).OrderBy(sr => sr.RequestTime);

            var vipOrdered = vips.ToList().Select((obj, index) =>
                new {songRequest = obj, playlistIndex = index * 2 + (isCurrentVip ? 0 : 1)});

            var regularOrdered = regulars.ToList().Select((obj, index) =>
                new {songRequest = obj, playlistIndex = index * 2 + (isCurrentVip ? 1 : 0)});

            var combined = vipOrdered.Union(regularOrdered);
            return combined.OrderBy(sr => sr.playlistIndex).ToList().Select(sr => sr.songRequest).ToList();
        }
    }
}
