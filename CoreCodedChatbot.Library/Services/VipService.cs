using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Database.Context.Models;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Library.Services
{
    public class VipService : IVipService
    {
        private IChatbotContextFactory chatbotContextFactory;
        private ConfigModel config;

        public VipService(IChatbotContextFactory chatbotContextFactory, IConfigService configService)
        {
            this.chatbotContextFactory = chatbotContextFactory;
            this.config = configService.GetConfig();
        }

        public bool GiftVip(string donorUsername, string receiverUsername)
        {
            var donorUser = GetUser(donorUsername);
            var receiverUser = GetUser(receiverUsername);

            if (donorUser == null || receiverUser == null) return false;

            return GiftVip(donorUser, receiverUser);
        }

        private bool GiftVip(User donor, User receiver)
        {
            try
            {
                using (var context = chatbotContextFactory.Create())
                {
                    var donorUser = context.Users.Find(donor);
                    var receiverUser = context.Users.Find(receiver);

                    donorUser.SentGiftVipRequests++;
                    receiverUser.ReceivedGiftVipRequests++;

                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e} - {e.InnerException}");
                return false;
            }

            return true;
        }

        private User GetUser(string username)
        {
            using (var context = chatbotContextFactory.Create())
            {
                return context.Users.Find(username.ToLower())
                    ?? this.AddUser(context, username, false);
            }
        }

        private User AddUser(IChatbotContext context, string username, bool deferSave)
        {
            var userModel = new User
            {
                Username = username.ToLower(),
                UsedVipRequests = 0,
                ModGivenVipRequests = 0,
                FollowVipRequest = 0,
                SubVipRequests = 0,
                DonationOrBitsVipRequests = 0,
                TokenBytes = 0,
                ReceivedGiftVipRequests = 0,
                SentGiftVipRequests = 0
            };

            try
            {
                context.Users.Add(userModel);
                if (!deferSave) context.SaveChanges();
            }
            catch (Exception)
            {
                return null;
            }

            return userModel;
        }
    }
}
