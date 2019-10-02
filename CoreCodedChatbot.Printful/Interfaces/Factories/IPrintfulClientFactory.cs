using CoreCodedChatbot.Printful.Interfaces.ExternalClients;

namespace CoreCodedChatbot.Printful.Interfaces.Factories
{
    public interface IPrintfulClientFactory
    {
        IPrintfulClient Get();
    }
}