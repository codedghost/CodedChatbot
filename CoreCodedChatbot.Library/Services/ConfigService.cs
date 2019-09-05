using System.IO;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;
using Newtonsoft.Json;

namespace CoreCodedChatbot.Library.Services
{
    public class ConfigService : IConfigService
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
