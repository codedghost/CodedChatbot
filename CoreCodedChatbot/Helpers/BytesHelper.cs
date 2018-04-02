using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Models.Data;

namespace CoreCodedChatbot.Helpers
{
    public static class BytesHelper
    {
        private const int bytesToVip = 300;

        public static void GiveBytes(ChatViewersModel chatViewersModel)
        {
            using (var context = new ChatbotContext())
            {
                if (chatViewersModel.chatters.moderators.Any()) VipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.moderators);
                if (chatViewersModel.chatters.staff.Any()) VipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.staff);
                if (chatViewersModel.chatters.global_mods.Any()) VipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.global_mods);
                if (chatViewersModel.chatters.admins.Any()) VipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.admins);
                if (chatViewersModel.chatters.viewers.Any()) VipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.viewers);

                context.SaveChanges();

                foreach (var mod in chatViewersModel.chatters.moderators)
                {
                    var user = VipHelper.FindUser(context, mod);
                    user.TokenBytes++;
                }
                foreach (var staff in chatViewersModel.chatters.staff)
                {
                    var user = VipHelper.FindUser(context, staff);
                    user.TokenBytes++;
                }
                foreach (var global_mod in chatViewersModel.chatters.global_mods)
                {
                    var user = VipHelper.FindUser(context, global_mod);
                    user.TokenBytes++;
                }
                foreach (var admin in chatViewersModel.chatters.admins)
                {
                    var user = VipHelper.FindUser(context, admin);
                    user.TokenBytes++;
                }
                foreach (var viewer in chatViewersModel.chatters.viewers)
                {
                    var user = VipHelper.FindUser(context, viewer);
                    user.TokenBytes++;
                }

                context.SaveChanges();
            }
        }

        public static string CheckBytes(string username)
        {
            using (var context = new ChatbotContext())
            {
                var user = VipHelper.FindUser(context, username);
                return (user.TokenBytes / (float)bytesToVip).ToString("n3");
            }
        }

        public static bool ConvertByte(string username)
        {
            using (var context = new ChatbotContext())
            {
                try
                {
                    var user = VipHelper.FindUser(context, username);
                    if (user.TokenBytes >= bytesToVip)
                    {
                        return VipHelper.GiveTokenVip(context, user, bytesToVip);
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
