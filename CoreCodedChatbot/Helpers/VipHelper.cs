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
    public class VipHelper
    {
        private readonly ChatbotContextFactory contextFactory;

        public VipHelper(ChatbotContextFactory contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        public User FindUser(string username, bool deferSave = false)
        {
            using (var context = this.contextFactory.Create())
            {
                return context.Users.Find(username.ToLower())
                    ?? this.AddUser(username, deferSave);
            }
        }

        public void AddUsersDeferSave(string[] usernames)
        {
            using (var context = this.contextFactory.Create())
            {
                var savedUsers = context.Users.Select(u => u.Username);
                var currentUsers = usernames.Where(u => !savedUsers.Contains(u));

                foreach (var user in currentUsers)
                {
                    this.AddUser(user, true);
                }
            }
        }

        private User AddUser(string username, bool deferSave)
        {
            using (var context = this.contextFactory.Create())
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
        }

        public bool GiveVipRequest(string username)
        {
            using (var context = this.contextFactory.Create())
            {
                var user = this.FindUser(username);

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

        public bool GiveFollowVip(string username)
        {
            using (var context = this.contextFactory.Create())
            {
                var user = this.FindUser(username);
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

        public bool StartupFollowVips(List<ChannelFollow> follows)
        {
            using (var context = this.contextFactory.Create())
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
                catch (Exception e)
                {
                    return false;
                }
                return true;
            }
        }

        public bool GiveSubVip(string username)
        {
            using (var context = this.contextFactory.Create())
            {
                var user = this.FindUser(username);
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

        public bool GiveDonationVip(string username, int totalBitsToDate)
        {
            using (var context = this.contextFactory.Create())
            {
                var user = this.FindUser(username);
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

        public bool GiveTokenVip(User user, int bytesToRemove)
        {
            try
            {
                using (var context = this.contextFactory.Create())
                {
                    user.TokenVipRequests++;
                    user.TokenBytes -= bytesToRemove;
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool CanUseVipRequest(string username)
        {
            using (var context = this.contextFactory.Create())
            {
                var user = this.FindUser(username);
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
                    var user = this.FindUser(username);
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

        public VipRequests GetVipRequests(string username)
        {
            var requests = new VipRequests();
            using (var context = this.contextFactory.Create())
            {
                var user = this.FindUser(username);
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
