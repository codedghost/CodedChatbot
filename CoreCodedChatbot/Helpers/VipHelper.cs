﻿using System;
using System.Collections.Generic;
using System.Linq;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Database.Context.Models;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using Microsoft.Extensions.Logging;
using NLog;
using TwitchLib.Api.V5.Models.Subscriptions;


namespace CoreCodedChatbot.Helpers
{
    public class VipHelper : IVipHelper
    {
        private readonly IChatbotContextFactory _contextFactory;
        private readonly IConfigService _configService;
        private readonly ILogger<IVipHelper> _logger;

        public VipHelper(
            IChatbotContextFactory contextFactory, 
            IConfigService configService,
            ILogger<IVipHelper> logger)
        {
            _contextFactory = contextFactory;
            _configService = configService;
            _logger = logger;
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
    }
}
