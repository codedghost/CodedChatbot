using CoreCodedChatbot.Database.Context.Interfaces;

namespace CoreCodedChatbot.Database.Context
{
    public class ChatbotContextFactory : IChatbotContextFactory
    {
        public IChatbotContext Create()
        {
            return new ChatbotContext();
        }
    }
}
