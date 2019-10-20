﻿using System.Collections.Generic;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Database.Context.Models;
using CoreCodedChatbot.Library.Models.Data;
using TwitchLib.Api.V5.Models.Subscriptions;

namespace CoreCodedChatbot.Interfaces
{
    public interface IVipHelper
    {
        User FindUser(IChatbotContext context, string username, bool deferSave = false);
        void AddUsersDeferSave(IChatbotContext context, string[] usernames);
        bool GiveVipRequest(string username);
        bool StartupSubVips(List<Subscription> subs);
        bool GiveSubVip(string username, int substreak = 1);
        bool GiveSubBombVips(string[] usernames);
        bool GiveBitsVip(string username, int totalBitsToDate);
        bool GiveDonationVipsDb(User user);
        bool GiveDonationVips(string username, bool deferSave = false);
        bool GiveTokenVip(IChatbotContext context, User user, int bytesToRemove);
        bool CanUseVipRequest(string username);
        bool CanUseSuperVipRequest(string username);
        bool UseVipRequest(string username);
        bool UseSuperVipRequest(string username);
        VipRequests GetVipRequests(string username);
    }
}