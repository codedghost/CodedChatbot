using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreCodedChatbot.Models.Data;
using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Database.Context.Models;
using TwitchLib.Models.API.v5.Channels;
using TwitchLib.Models.API.v5.Subscriptions;

namespace CoreCodedChatbot.Helpers
{
    public static class VipHelper
    {
        public static User FindUser(ChatbotContext context, string username, bool deferSave = false)
        {
            return context.Users.Find(username.ToLower()) ??
                AddUser(context, username, deferSave);
        }

        public static void AddUsersDeferSave(ChatbotContext context, string[] usernames)
        {
            var savedUsers = context.Users.Select(u => u.Username);
            var currentUsers = usernames.Where(u => !savedUsers.Contains(u));

            foreach (var user in currentUsers)
            {
                AddUser(context, user, true);
            }
        }

        private static User AddUser(ChatbotContext context, string username, bool deferSave)
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
            catch (Exception e)
            {
                return null;
            }

            return userModel;
        }
        
        public static bool GiveVipRequest(string username)
        {
            using (var context = new ChatbotContext())
            {
                var user = FindUser(context, username);

                if (user == null) return false;
                try
                {
                    user.ModGivenVipRequests++;
                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    return false;
                }

                return true;
            }
        }

        public static bool GiveFollowVip(string username)
        {
            using (var context = new ChatbotContext())
            {
                var user = FindUser(context, username);
                if (user == null) return false;

                try
                {
                    user.FollowVipRequest = 1;
                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    return false;
                }

                return true;
            }
        }

        public static bool StartupFollowVips(List<ChannelFollow> follows)
        {
            using (var context = new ChatbotContext())
            {
                try
                {
                    var usernames = follows.Select(f => f.User.DisplayName.ToLower());
                    var currentRecords = context.Users.Where(u => usernames.Contains(u.Username));
                    foreach (var currentRecord in currentRecords)
                    {
                        if (currentRecord.FollowVipRequest != 1) currentRecord.FollowVipRequest = 1;
                    }

                    var otherRecords =
                        usernames.Where(u => !currentRecords.Select(c => c.Username.ToLower()).Contains(u));
                    var models = otherRecords.Select(or => new User
                    {
                        Username = or,
                        ModGivenVipRequests = 0,
                        FollowVipRequest = 1,
                        DonationOrBitsVipRequests = 0,
                        SubVipRequests = 0,
                        UsedVipRequests = 0,
                        TokenBytes = 0
                    });

                    context.Users.AddRange(models);
                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool StartupSubVips(List<Subscription> subs)
        {
            using (var context = new ChatbotContext())
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
                catch (Exception e)
                {
                    return false;
                }
                return true;
            }
        }

        public static bool GiveSubVip(string username)
        {
            using (var context = new ChatbotContext())
            {
                var user = FindUser(context, username);
                if (user == null) return false;

                try
                {
                    user.SubVipRequests++;
                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    return false;
                }

                return true;
            }
        }

        public static bool GiveDonationVip(string username, int totalBitsToDate)
        {
            using (var context = new ChatbotContext())
            {
                var user = FindUser(context, username);
                if (user == null) return false;

                try
                {
                    user.DonationOrBitsVipRequests = totalBitsToDate / 300;
                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    return false;
                }

                return true;
            }
        }

        public static bool GiveTokenVip(ChatbotContext context, User user, int bytesToRemove)
        {
            try
            {
                user.TokenVipRequests++;
                user.TokenBytes -= bytesToRemove;
                context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool CanUseVipRequest(string username)
        {
            using (var context = new ChatbotContext())
            {
                var user = FindUser(context, username);
                if (user == null ||
                    user.UsedVipRequests >=
                    (user.FollowVipRequest + user.SubVipRequests + user.ModGivenVipRequests +
                        user.DonationOrBitsVipRequests + user.TokenVipRequests)) return false;

                return true;
            }
        }

        public static bool UseVipRequest(string username)
        {
            if (!CanUseVipRequest(username))
            {
                return false;
            }

            using (var context = new ChatbotContext())
            {
                try
                {
                    var user = FindUser(context, username);
                    user.UsedVipRequests++;
                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            return true;
        }

        public static VipRequests GetVipRequests(string username)
        {
            var requests = new VipRequests();
            using (var context = new ChatbotContext())
            {
                var user = FindUser(context, username);
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
