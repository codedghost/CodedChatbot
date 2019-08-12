using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Database.Context.Models;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Library.Services
{
    public class UserService : IUserService
    {
        private IChatbotContextFactory chatbotContextFactory;

        private ConfigModel config;

        public UserService(IChatbotContextFactory chatbotContextFactory, IConfigService configService)
        {
            this.chatbotContextFactory = chatbotContextFactory;
            this.config = configService.GetConfig();
        }

        public VipRequests GetUserVipByteBalance(string username)
        {
            try
            {
                using (var context = chatbotContextFactory.Create())
                {
                    var user = context.Users.Find(username.ToLower());
                    return VipRequests.Create(user);
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
