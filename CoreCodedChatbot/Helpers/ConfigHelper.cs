using System.IO;

using CoreCodedChatbot.Helpers.Interfaces;
using CoreCodedChatbot.Library.Models.Data;
using Newtonsoft.Json;

namespace CoreCodedChatbot.Helpers
{
    public class ConfigHelper : IConfigHelper
    {
        public ConfigModel GetConfig()
        {
            using (var sr = new StreamReader("config.json"))
            {
                var configJson = sr.ReadToEnd();
                return JsonConvert.DeserializeObject<ConfigModel>(configJson);
            }
        }
    }
}
