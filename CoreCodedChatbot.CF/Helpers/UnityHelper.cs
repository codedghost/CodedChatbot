using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Services;
using Unity;

namespace CoreCodedChatbot.CF.Helpers
{
    public static class UnityHelper
    {
        public static IUnityContainer Create()
        {
            var container = new UnityContainer();

            container.RegisterType<IConfigService, ConfigService>();

            container.RegisterType<IChatbotContextFactory, ChatbotContextFactory>();

            return container;
        }
    }
}
