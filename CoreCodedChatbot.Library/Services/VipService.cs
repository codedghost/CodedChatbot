using System;
using System.Linq;
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

        public bool RefundVip(string username, bool deferSave = false)
        {
            try
            {
                using (var context = chatbotContextFactory.Create())
                {
                    var user = context.Users.SingleOrDefault(u => u.Username == username);

                    if (user == null) return false;

                    user.ModGivenVipRequests++;

                    if (!deferSave) context.SaveChanges();
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e} - {e.InnerException}");
                return false;
            }
        }

        public bool RefundSuperVip(string username, bool deferSave = false)
        {
            try
            {
                using (var context = chatbotContextFactory.Create())
                {
                    var user = context.Users.SingleOrDefault(u => u.Username == username);

                    if (user == null) return false;

                    user.ModGivenVipRequests += config.SuperVipCost;

                    if (!deferSave) context.SaveChanges();
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e} - {e.InnerException}");
                return false;
            }
        }

        public bool HasVip(string username)
        {
            try
            {
                var user = GetUser(username);

                return user != null && VipRequests.Create(user, config).TotalRemaining > 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("HasVip Exception:");
                Console.WriteLine($"{e} - {e.InnerException}");
                return false;
            }
        }

        public bool UseVip(string username)
        {
            try
            {
                if (!HasVip(username)) return false;

                using (var context = chatbotContextFactory.Create())
                {
                    var user = context.Users.Find(username);

                    user.UsedVipRequests++;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"UseVIP Exception\n{e} - {e.InnerException}");
                return false;
            }

            return true;
        }

        public bool HasSuperVip(string username)
        {
            try
            {
                var user = GetUser(username);

                return user != null && VipRequests.Create(user, config).TotalRemaining > config.SuperVipCost;
            }
            catch (Exception e)
            {
                Console.WriteLine("HasSuperVip Exception:");
                Console.WriteLine($"{e} - {e.InnerException}");
                return false;
            }
        }

        public bool UseSuperVip(string username)
        {
            try
            {
                if (!HasSuperVip(username)) return false;

                using (var context = chatbotContextFactory.Create())
                {
                    var user = context.Users.Find(username);

                    user.UsedSuperVipRequests++;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"UseSuperVIP Exception\n{e} - {e.InnerException}");
                return false;
            }

            return true;
        }

        public bool ModGiveVip(string username, int numberOfVips)
        {
            try
            {
                using (var context = chatbotContextFactory.Create())
                {
                    var user = GetUser(username);

                    user.ModGivenVipRequests += numberOfVips;
                    context.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"ModGiveVIP Exception\n{e} - {e.InnerException}");
                return false;
            }

            return true;
        }

        private bool GiftVip(User donor, User receiver)
        {
            try
            {
                if (!HasVip(donor.Username)) return false;

                using (var context = chatbotContextFactory.Create())
                {
                    var donorUser = context.Users.Find(donor.Username);
                    var receiverUser = context.Users.Find(receiver.Username);



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

        private User GetUser(string username, bool createUser = true)
        {
            using (var context = chatbotContextFactory.Create())
            {
                var user = context.Users.Find(username.ToLower());

                if (user == null && createUser)
                    user = this.AddUser(context, username, false);

                return user;
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
