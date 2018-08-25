using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CoreCodedChatbot.Library.Helpers
{
    public static class HttpClientHelper
    {
        public static ByteArrayContent GetJsonData(object data)
        {
            var jsonData = JsonConvert.SerializeObject(data);
            var byteContent = new ByteArrayContent(Encoding.UTF8.GetBytes(jsonData));
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return byteContent;
        }
    }
}
