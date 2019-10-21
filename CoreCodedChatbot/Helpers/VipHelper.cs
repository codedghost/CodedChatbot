using System;
using System.Collections.Generic;
using System.Linq;

using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Database.Context.Models;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using TwitchLib.Api.V5.Models.Subscriptions;


namespace CoreCodedChatbot.Helpers
{
    public class VipHelper : IVipHelper
    {
        private readonly IChatbotContextFactory _contextFactory;
        private readonly IConfigService _configService;

        public VipHelper(IChatbotContextFactory contextFactory, IConfigService configService)
        {
            _contextFactory = contextFactory;
            _configService = configService;
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

        public bool GiveVipRequest(string username)
        {
            using (var context = this._contextFactory.Create())
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
            using (var context = this._contextFactory.Create())
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
                        TokenBytes = 0,
                        ReceivedGiftVipRequests = 0,
                        SentGiftVipRequests = 0
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
            using (var context = this._contextFactory.Create())
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

        public bool GiveSubBombVips(string[] usernames)
        {
            using (var context = _contextFactory.Create())
            {
                try
                {
                    foreach (var username in usernames)
                    {
                        var user = FindUser(context, username);
                        if (user == null) return false;

                        user.SubVipRequests++;

                    }

                    context.SaveChanges();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool GiveBitsVip(string username, int totalBitsToDate)
        {
            using (var context = this._contextFactory.Create())
            {
                var user = this.FindUser(context, username);
                if (user == null) return false;

                try
                {
                    user.TotalBitsDropped = totalBitsToDate;
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }
        }

        public bool GiveDonationVipsDb(User user)
        {
            try
            {
                var totalBitsGiven = user.TotalBitsDropped;
                var totalDonated = user.TotalDonated;

                var bitsVipPercentage = (double) totalBitsGiven / _configService.Get<double>("BitsToVip");
                var donationVipPercentage = (double) totalDonated / _configService.Get<double>("DonationAmountToVip");

                user.DonationOrBitsVipRequests = (int) Math.Floor(bitsVipPercentage + donationVipPercentage);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool GiveDonationVips(string username, bool deferSave = false)
        {
            try
            {
                using (var context = _contextFactory.Create())
                {
                    var user = FindUser(context, username);
                    GiveDonationVipsDb(user);
                    if (deferSave) return true;
                    context.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
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
            using (var context = this._contextFactory.Create())
            {
                var user = this.FindUser(context, username);
                return user != null && new VipRequests(_configService, user).TotalRemaining > 0;
            }
        }

        public bool CanUseSuperVipRequest(string username)
        {
            using (var context = _contextFactory.Create())
            {
                var user = FindUser(context, username);
                return user != null && (new VipRequests(_configService, user)).TotalRemaining >= _configService.Get<int>("SuperVipCost");
            }
        }

        public bool UseVipRequest(string username)
        {
            if (!CanUseVipRequest(username))
            {
                return false;
            }

            using (var context = this._contextFactory.Create())
            {
                try
                {
                    var user = this.FindUser(context, username);
                    user.UsedVipRequests++;
                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e} - {e.InnerException}");
                    return false;
                }
            }
            return true;
        }

        public bool UseSuperVipRequest(string username)
        {
            if (!CanUseSuperVipRequest(username))
            {
                return false;
            }

            using (var context = _contextFactory.Create())
            {
                try
                {
                    var user = FindUser(context, username);
                    user.UsedSuperVipRequests++;
                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e} - {e.InnerException}");
                    return false;
                }
            }

            return true;
        }

        public VipRequests GetVipRequests(string username)
        {
            using (var context = this._contextFactory.Create())
            {
                var user = this.FindUser(context, username);
                return user == null ? null : new VipRequests(_configService, user);
            }
        }
    }
}
