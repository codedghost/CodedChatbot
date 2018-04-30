﻿using System;
using System.Linq;

using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Helpers.Interfaces;
using CoreCodedChatbot.Models.Data;

namespace CoreCodedChatbot.Helpers
{
    public class BytesHelper
    {
        private readonly IChatbotContextFactory contextFactory;

        private readonly VipHelper vipHelper;
        private readonly ConfigModel config;

        public BytesHelper(IChatbotContextFactory contextFactory, VipHelper vipHelper, IConfigHelper configHelper)
        {
            this.contextFactory = contextFactory;
            this.vipHelper = vipHelper;
            this.config = configHelper.GetConfig();
        }

        public void GiveBytes(ChatViewersModel chatViewersModel)
        {
            using (var context = this.contextFactory.Create())
            {
                if (chatViewersModel.chatters.moderators.Any()) vipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.moderators);
                if (chatViewersModel.chatters.staff.Any()) vipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.staff);
                if (chatViewersModel.chatters.global_mods.Any()) vipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.global_mods);
                if (chatViewersModel.chatters.admins.Any()) vipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.admins);
                if (chatViewersModel.chatters.viewers.Any()) vipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.viewers);

                context.SaveChanges();

                foreach (var mod in chatViewersModel.chatters.moderators)
                {
                    var user = vipHelper.FindUser(context, mod);
                    user.TokenBytes++;
                }
                foreach (var staff in chatViewersModel.chatters.staff)
                {
                    var user = vipHelper.FindUser(context, staff);
                    user.TokenBytes++;
                }
                foreach (var global_mod in chatViewersModel.chatters.global_mods)
                {
                    var user = vipHelper.FindUser(context, global_mod);
                    user.TokenBytes++;
                }
                foreach (var admin in chatViewersModel.chatters.admins)
                {
                    var user = vipHelper.FindUser(context, admin);
                    user.TokenBytes++;
                }
                foreach (var viewer in chatViewersModel.chatters.viewers)
                {
                    var user = vipHelper.FindUser(context, viewer);
                    user.TokenBytes++;
                }

                context.SaveChanges();
            }
        }

        public string CheckBytes(string username)
        {
            using (var context = this.contextFactory.Create())
            {
                var user = vipHelper.FindUser(context, username);
                return (user.TokenBytes / (float)config.BytesToVip).ToString("n3");
            }
        }

        public bool ConvertByte(string username, int tokensToConvert = 1)
        {
            using (var context = this.contextFactory.Create())
            {
                try
                {
                    var user = vipHelper.FindUser(context, username);
                    if ((user.TokenBytes * tokensToConvert) >= config.BytesToVip)
                    {
                        for (int i = 0; i < tokensToConvert; i++)
                        {
                            if (!vipHelper.GiveTokenVip(context, user, config.BytesToVip))
                            {
                                return false;
                            }
                        }

                        return true;
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
