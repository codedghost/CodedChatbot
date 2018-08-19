using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Web.Interfaces
{
    public interface IChatterService
    {
        void UpdateChatters();
        ChatViewersModel GetCurrentChatters();
    }
}
