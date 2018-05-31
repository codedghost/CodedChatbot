using System;
using System.Collections.Generic;
using System.Linq;

using CoreCodedChatbot.Database.Context.Models;

namespace CoreCodedChatbot.Extensions
{
    public static class IQueryableHelper
    {
        public static List<SongRequest> OrderRequests(this IQueryable<SongRequest> requests)
        {
            return requests.OrderBy(sr => sr.VipRequestTime ?? DateTime.MaxValue).ThenBy(sr => sr.RequestTime).ToList();
        }
    }
}
