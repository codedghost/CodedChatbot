using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Models.Data;
using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Database.Context.Models;
using TwitchLib.Api.Models.v5.Subscriptions;


namespace CoreCodedChatbot.Helpers
{
    public class VipHelper
    {
        private readonly ChatbotContextFactory contextFactory;
        private readonly ConfigModel config;

        public VipHelper(ChatbotContextFactory contextFactory, ConfigModel config)
        {
            this.contextFactory = contextFactory;
            this.config = config;
        }

        public User FindUser(IChatbotContext context, string username, bool deferSave = false)
        {
            return context.Users.Find(username.ToLower())
                ?? this.AddUser(context, username, deferSave);
        }

        public void AddUsersDeferSave(IChatbotContext context, string[] usernames)
        {
            var savedUsers = context.Users.Select(u => u.Username);
            var currentUsers = usernames.Where(u => !savedUsers.Contains(u));

            foreach (var user in currentUsers)
            {
                this.AddUser(context, user, true);
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
                TokenBytes = 0
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

        public bool GiveVipRequest(string username)
        {
            using (var context = this.contextFactory.Create())
            {
                var user = this.FindUser(context, username);

                if (user == null) return false;
                try
                {
                    user.ModGivenVipRequests++;
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }
        }

        public bool StartupSubVips(List<Subscription> subs)
        {
            using (var context = this.contextFactory.Create())
            {
                try
                {
                    var usernames = subs.Select(s => s.User.DisplayName.ToLower());
                    var currentRecords = context.Users.Where(u => usernames.Contains(u.Username));
                    foreach (var currentRecord in currentRecords)
                    {
                        if (currentRecord.SubVipRequests == 0) currentRecord.SubVipRequests = 1;
                    }

                    var otherRecords =
                        usernames.Where(u => !currentRecords.Select(c => c.Username.ToLower()).Contains(u));
                    var models = otherRecords.Select(or => new User
                    {
                        Username = or,
                        ModGivenVipRequests = 0,
                        FollowVipRequest = 0,
                        DonationOrBitsVipRequests = 0,
                        SubVipRequests = 1,
                        UsedVipRequests = 0,
                        TokenBytes = 0
                    });

                    context.Users.AddRange(models);
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }
        }

        public bool GiveSubVip(string username, int subStreak = 1)
        {
            using (var context = this.contextFactory.Create())
            {
                var user = this.FindUser(context, username);
                if (user == null) return false;

                try
                {
                    user.SubVipRequests++;
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }
        }

        public bool GiveDonationVip(string username, int totalBitsToDate)
        {
            using (var context = this.contextFactory.Create())
            {
                var user = this.FindUser(context, username);
                if (user == null) return false;

                try
                {
                    user.DonationOrBitsVipRequests = totalBitsToDate / config.BitsToVip;
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }
        }

        public bool GiveTokenVip(IChatbotContext context, User user, int bytesToRemove)
        {
            try
            {
                user.TokenVipRequests++;
                user.TokenBytes -= bytesToRemove;
                context.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool CanUseVipRequest(string username)
        {
            using (var context = this.contextFactory.Create())
            {
                var user = this.FindUser(context, username);
                if (user == null ||
                    user.UsedVipRequests >=
                    (user.FollowVipRequest + user.SubVipRequests + user.ModGivenVipRequests +
                        user.DonationOrBitsVipRequests + user.TokenVipRequests)) return false;

                return true;
            }
        }

        public bool UseVipRequest(string username)
        {
            if (!CanUseVipRequest(username))
            {
                return false;
            }

            using (var context = this.contextFactory.Create())
            {
                try
                {
                    var user = this.FindUser(context, username);
                    user.UsedVipRequests++;
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        public VipRequests GetVipRequests(string username)
        {
            var requests = new VipRequests();
            using (var context = this.contextFactory.Create())
            {
                var user = this.FindUser(context, username);
                if (user == null) return requests;

                requests = new VipRequests
                {
                    Donations = user.DonationOrBitsVipRequests,
                    Follow = user.FollowVipRequest,
                    ModGiven = user.ModGivenVipRequests,
                    Sub = user.SubVipRequests,
                    Used = user.UsedVipRequests,
                    Byte = user.TokenVipRequests
                };
            }

            return requests;
        }
    }
}
