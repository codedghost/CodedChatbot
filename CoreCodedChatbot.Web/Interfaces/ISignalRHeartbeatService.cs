using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodedChatbot.Web.Interfaces
{
    public interface ISignalRHeartbeatService
    {
        void OnTimerCallback(object state);
        void NotifyClients();
    }
}
