using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodedChatbot.Web.Models
{
    public class LoggedInTwitchUser
    {
        public bool IsMod { get; set; }
        public string Username { get; set; }
    }
}
