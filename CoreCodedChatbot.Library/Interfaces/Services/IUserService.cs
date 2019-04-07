using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Database.Context.Models;
using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Library.Interfaces.Services
{
    public interface IUserService
    {
        UserBalance GetUserVipByteBalance(string username);
    }
}
