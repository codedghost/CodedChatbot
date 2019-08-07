using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreCodedChatbot.Database.Context.Models;

namespace CoreCodedChatbot.Library.Extensions
{
    public static class SongListQueryableExtensions
    {
        public static List<SongRequest> OrderRequests(this IQueryable<SongRequest> requests)
        {
            return requests.OrderBy(sr => sr.SuperVipRequestTime ?? DateTime.MaxValue).ThenBy(sr => sr.VipRequestTime ?? DateTime.MaxValue).ThenBy(sr => sr.RequestTime).ToList();
        }
    }
}
