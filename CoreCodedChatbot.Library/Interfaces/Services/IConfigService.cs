using System;
using System.Collections.Generic;
using System.Text;
using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Library.Interfaces.Services
{
    public interface IConfigService
    {
        ConfigModel GetConfig();
    }
}
