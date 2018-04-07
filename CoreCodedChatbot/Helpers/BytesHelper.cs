﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Models.Data;

namespace CoreCodedChatbot.Helpers
{
    public class BytesHelper
    {
        private readonly ChatbotContextFactory contextFactory;

        private readonly VipHelper vipHelper;

        public BytesHelper(ChatbotContextFactory contextFactory, VipHelper vipHelper)
        {
            this.contextFactory = contextFactory;
            this.vipHelper = vipHelper;
        }

        private const int bytesToVip = 300;

        public void GiveBytes(ChatViewersModel chatViewersModel)
        {
            using (var context = this.contextFactory.Create())
            {
                if (chatViewersModel.chatters.moderators.Any()) vipHelper.AddUsersDeferSave(chatViewersModel.chatters.moderators);
                if (chatViewersModel.chatters.staff.Any()) vipHelper.AddUsersDeferSave(chatViewersModel.chatters.staff);
                if (chatViewersModel.chatters.global_mods.Any()) vipHelper.AddUsersDeferSave(chatViewersModel.chatters.global_mods);
                if (chatViewersModel.chatters.admins.Any()) vipHelper.AddUsersDeferSave(chatViewersModel.chatters.admins);
                if (chatViewersModel.chatters.viewers.Any()) vipHelper.AddUsersDeferSave(chatViewersModel.chatters.viewers);

                context.SaveChanges();

                foreach (var mod in chatViewersModel.chatters.moderators)
                {
                    var user = vipHelper.FindUser(mod);
                    user.TokenBytes++;
                }
                foreach (var staff in chatViewersModel.chatters.staff)
                {
                    var user = vipHelper.FindUser(staff);
                    user.TokenBytes++;
                }
                foreach (var global_mod in chatViewersModel.chatters.global_mods)
                {
                    var user = vipHelper.FindUser(global_mod);
                    user.TokenBytes++;
                }
                foreach (var admin in chatViewersModel.chatters.admins)
                {
                    var user = vipHelper.FindUser(admin);
                    user.TokenBytes++;
                }
                foreach (var viewer in chatViewersModel.chatters.viewers)
                {
                    var user = vipHelper.FindUser(viewer);
                    user.TokenBytes++;
                }

                context.SaveChanges();
            }
        }

        public string CheckBytes(string username)
        {
            using (var context = this.contextFactory.Create())
            {
                var user = vipHelper.FindUser(username);
                return (user.TokenBytes / (float)bytesToVip).ToString("n3");
            }
        }

        public bool ConvertByte(string username)
        {
            using (var context = this.contextFactory.Create())
            {
                try
                {
                    var user = vipHelper.FindUser(username);
                    if (user.TokenBytes >= bytesToVip)
                    {
                        return vipHelper.GiveTokenVip(user, bytesToVip);
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
    }
}
