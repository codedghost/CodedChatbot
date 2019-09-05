namespace CoreCodedChatbot.Web.Interfaces
{
    public interface ISignalRHeartbeatService
    {
        void OnTimerCallback(object state);
        void NotifyClients();
    }
}
