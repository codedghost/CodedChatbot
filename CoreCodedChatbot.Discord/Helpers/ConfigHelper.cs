using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Discord.Helpers.Interfaces;
using CoreCodedChatbot.Discord.Models.DataModels;
using Newtonsoft.Json;

namespace CoreCodedChatbot.Discord.Helpers
{
    public class ConfigHelper : IConfigHelper
    {
        public Config GetConfig()
        {
            using (var sr = new StreamReader("config.json"))
            {
                var configJson = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<Config>(configJson);
            }
        }
    }
}
