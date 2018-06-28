using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Discord.Models.DataModels;

namespace CoreCodedChatbot.Discord.Helpers.Interfaces
{
    public interface IConfigHelper
    {
        Config GetConfig();
    }
}
