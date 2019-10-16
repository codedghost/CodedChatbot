using System;
using System.Linq;

using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Helpers
{
    public class BytesHelper : IBytesHelper
    {
        private readonly IChatbotContextFactory contextFactory;

        private readonly IVipHelper vipHelper;
        private readonly ConfigModel config;

        public BytesHelper(IChatbotContextFactory contextFactory, IVipHelper vipHelper, IConfigService configService)
        {
            this.contextFactory = contextFactory;
            this.vipHelper = vipHelper;
            this.config = configService.GetConfig();
        }

        public void GiveViewershipBytes(ChatViewersModel chatViewersModel)
        {
            using (var context = this.contextFactory.Create())
            {
                if (chatViewersModel.chatters.moderators.Any()) vipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.moderators);
                if (chatViewersModel.chatters.staff.Any()) vipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.staff);
                if (chatViewersModel.chatters.global_mods.Any()) vipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.global_mods);
                if (chatViewersModel.chatters.admins.Any()) vipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.admins);
                if (chatViewersModel.chatters.vips.Any()) vipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.vips);
                if (chatViewersModel.chatters.viewers.Any()) vipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.viewers);

                context.SaveChanges();

                foreach (var mod in chatViewersModel.chatters.moderators)
                {
                    var user = vipHelper.FindUser(context, mod);
                    user.TokenBytes++;
                    user.TimeLastInChat = DateTime.UtcNow;
                }
                foreach (var staff in chatViewersModel.chatters.staff)
                {
                    var user = vipHelper.FindUser(context, staff);
                    user.TokenBytes++;
                    user.TimeLastInChat = DateTime.UtcNow;
                }
                foreach (var global_mod in chatViewersModel.chatters.global_mods)
                {
                    var user = vipHelper.FindUser(context, global_mod);
                    user.TokenBytes++;
                    user.TimeLastInChat = DateTime.UtcNow;
                }
                foreach (var admin in chatViewersModel.chatters.admins)
                {
                    var user = vipHelper.FindUser(context, admin);
                    user.TokenBytes++;
                    user.TimeLastInChat = DateTime.UtcNow;
                }

                foreach (var vip in chatViewersModel.chatters.vips)
                {
                    var user = vipHelper.FindUser(context, vip);
                    user.TokenBytes++;
                    user.TimeLastInChat = DateTime.UtcNow;
                }
                foreach (var viewer in chatViewersModel.chatters.viewers)
                {
                    var user = vipHelper.FindUser(context, viewer);
                    user.TokenBytes++;
                    user.TimeLastInChat = DateTime.UtcNow;
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
                    if (tokensToConvert < 0) return false;
                    if ((user.TokenBytes * tokensToConvert) >= config.BytesToVip * tokensToConvert)
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
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public bool ConvertAllBytes(string username)
        {
            var totalBytes = 0;
            using (var context = contextFactory.Create())
            {
                try
                {
                    var user = vipHelper.FindUser(context, username);
                    totalBytes = user.TokenBytes / config.BytesToVip;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return ConvertByte(username, totalBytes);
        }

        public bool GiveGiftSubBytes(string username, int subCount = 1)
        {
            using (var context = contextFactory.Create())
            {
                try
                {
                    var user = vipHelper.FindUser(context, username);
                    var totalBytes = (config.BytesToVip / 2) * subCount;

                    user.TokenBytes += totalBytes;

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
}
