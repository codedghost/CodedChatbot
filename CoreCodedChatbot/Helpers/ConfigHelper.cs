using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CoreCodedChatbot.Models.Data;
using Newtonsoft.Json;

namespace CoreCodedChatbot.Helpers
{
    public static class ConfigHelper
    {
        public static ConfigModel GetConfig()
        {
            using (var sr = new StreamReader("config.json"))
            {
                var configJson = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<ConfigModel>(configJson);
            }
        }
    }
}
