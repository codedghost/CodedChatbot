using CoreCodedChatbot.Database.Context.Interfaces;

namespace CoreCodedChatbot.Database.Context
{
    public class ChatbotContextFactory
    {
        public IChatbotContext Create()
        {
            return new ChatbotContext();
        }
    }
}
