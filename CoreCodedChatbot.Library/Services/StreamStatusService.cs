using System;
using System.Linq;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Database.Context.Models;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.ApiRequest.StreamStatus;

namespace CoreCodedChatbot.Library.Services
{
    public class StreamStatusService : IStreamStatusService
    {
        private readonly IChatbotContextFactory _chatbotContextFactory;

        public StreamStatusService(IChatbotContextFactory chatbotContextFactory)
        {
            _chatbotContextFactory = chatbotContextFactory;
        }

        public bool GetStreamStatus(string broadcasterUsername)
        {
            using (var context = _chatbotContextFactory.Create())
            {
                var status = context.StreamStatuses.FirstOrDefault(s =>
                    s.BroadcasterUsername.Equals(broadcasterUsername, StringComparison.CurrentCultureIgnoreCase));

                return status?.IsOnline ?? false;
            }
        }

        public bool SaveStreamStatus(PutStreamStatusRequest putStreamStatusRequest)
        {
            try
            {
                using (var context = _chatbotContextFactory.Create())
                {
                    var currentStatus = context.StreamStatuses.FirstOrDefault(s =>
                        s.BroadcasterUsername.Equals(putStreamStatusRequest.BroadcasterUsername,
                            StringComparison.CurrentCultureIgnoreCase));

                    if (currentStatus == null)
                    {
                        currentStatus = new StreamStatus
                        {
                            BroadcasterUsername = putStreamStatusRequest.BroadcasterUsername,
                            IsOnline = putStreamStatusRequest.IsOnline
                        };

                        context.StreamStatuses.Add(currentStatus);
                        context.SaveChanges();
                        return true;
                    }

                    currentStatus.IsOnline = putStreamStatusRequest.IsOnline;
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(
                    $"Exception caught when saving Stream Status. Exception: {e} - {e.InnerException}");

                return false;
            }
        }
    }
}