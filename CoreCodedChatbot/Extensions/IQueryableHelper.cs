using System;
using System.Collections.Generic;
using System.Linq;

using CoreCodedChatbot.Database.Context.Models;

namespace CoreCodedChatbot.Extensions
{
    public static class IQueryableHelper
    {
        public static List<SongRequest> OrderRequests(this IQueryable<SongRequest> requests, bool isCurrentVip)
        {
            var newRequestList = new List<SongRequest>();
            var vipsFirst = requests.OrderBy(sr => sr.VipRequestTime ?? DateTime.MaxValue).ThenBy(sr => sr.RequestTime).ToList();
            var vipRequestCount = requests.Count(sr => sr.VipRequestTime != null);

            var vipsAdded = 0;
            var regularsAdded = 0;

            for (var i = 0; i < vipsFirst.Count(); i++)
            {
                if (i % 2 == 0)
                {
                    if (isCurrentVip)
                    {
                        if (i - regularsAdded < vipRequestCount)
                        {
                            newRequestList.Add(vipsFirst[i - regularsAdded]);
                        }
                        else
                        {
                            newRequestList.Add(vipsFirst[i + vipRequestCount]);
                        }
                    }
                    else
                    {
                        if (i + vipRequestCount - vipsAdded < vipsFirst.Count() - vipRequestCount)
                        {
                            newRequestList.Add(vipsFirst[(i + vipRequestCount) - vipsAdded]);
                        }
                        else
                        {
                            newRequestList.Add();
                        }
                    }
                }
                else
                {
                    if (isCurrentVip)
                    {
                        newRequestList.Add(vipsFirst[(i + vipRequestCount) - vipsAdded]);
                    }
                    else
                    {
                        newRequestList.Add(vipsFirst[i - regularsAdded]);
                    }
                }
            }

            return newRequestList;
        }
    }
}
