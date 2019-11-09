using System;
using System.Linq;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Helpers
{
    public class BytesHelper : IBytesHelper
    {
        private readonly IChatbotContextFactory _contextFactory;

        private readonly IVipHelper _vipHelper;
        private readonly IConfigService _configService;

        public BytesHelper(IChatbotContextFactory contextFactory, IVipHelper vipHelper, IConfigService configService)
        {
            this._contextFactory = contextFactory;
            this._vipHelper = vipHelper;
            _configService = configService;
        }

        public void GiveViewershipBytes(ChatViewersModel chatViewersModel)
        {
            using (var context = this._contextFactory.Create())
            {
                if (chatViewersModel.chatters.moderators.Any()) _vipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.moderators);
                if (chatViewersModel.chatters.staff.Any()) _vipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.staff);
                if (chatViewersModel.chatters.global_mods.Any()) _vipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.global_mods);
                if (chatViewersModel.chatters.admins.Any()) _vipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.admins);
                if (chatViewersModel.chatters.vips.Any()) _vipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.vips);
                if (chatViewersModel.chatters.viewers.Any()) _vipHelper.AddUsersDeferSave(context, chatViewersModel.chatters.viewers);

                context.SaveChanges();

                foreach (var mod in chatViewersModel.chatters.moderators)
                {
                    var user = _vipHelper.FindUser(context, mod);
                    user.TokenBytes++;
                    user.TimeLastInChat = DateTime.UtcNow;
                }
                foreach (var staff in chatViewersModel.chatters.staff)
                {
                    var user = _vipHelper.FindUser(context, staff);
                    user.TokenBytes++;
                    user.TimeLastInChat = DateTime.UtcNow;
                }
                foreach (var global_mod in chatViewersModel.chatters.global_mods)
                {
                    var user = _vipHelper.FindUser(context, global_mod);
                    user.TokenBytes++;
                    user.TimeLastInChat = DateTime.UtcNow;
                }
                foreach (var admin in chatViewersModel.chatters.admins)
                {
                    var user = _vipHelper.FindUser(context, admin);
                    user.TokenBytes++;
                    user.TimeLastInChat = DateTime.UtcNow;
                }

                foreach (var vip in chatViewersModel.chatters.vips)
                {
                    var user = _vipHelper.FindUser(context, vip);
                    user.TokenBytes++;
                    user.TimeLastInChat = DateTime.UtcNow;
                }
                foreach (var viewer in chatViewersModel.chatters.viewers)
                {
                    var user = _vipHelper.FindUser(context, viewer);
                    user.TokenBytes++;
                    user.TimeLastInChat = DateTime.UtcNow;
                }

                context.SaveChanges();
            }
        }

        public string CheckBytes(string username)
        {
            using (var context = this._contextFactory.Create())
            {
                var user = _vipHelper.FindUser(context, username);
                return (user.TokenBytes / _configService.Get<float>("BytesToVip")).ToString("n3");
            }
        }

        public bool ConvertByte(string username, int tokensToConvert = 1)
        {
            using (var context = this._contextFactory.Create())
            {
                try
                {
                    var user = _vipHelper.FindUser(context, username);
                    if (tokensToConvert < 0) return false;
                    if ((user.TokenBytes * tokensToConvert) >= _configService.Get<int>("BytesToVip") * tokensToConvert)
                    {
                        for (int i = 0; i < tokensToConvert; i++)
                        {
                            if (!_vipHelper.GiveTokenVip(context, user, _configService.Get<int>("BytesToVip")))
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
            using (var context = _contextFactory.Create())
            {
                try
                {
                    var user = _vipHelper.FindUser(context, username);
                    totalBytes = user.TokenBytes / _configService.Get<int>("BytesToVip");
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
            using (var context = _contextFactory.Create())
            {
                try
                {
                    var user = _vipHelper.FindUser(context, username);
                    var totalBytes = (_configService.Get<int>("BytesToVip") / 2) * subCount;

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
