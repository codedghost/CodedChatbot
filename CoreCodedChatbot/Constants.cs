using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace CoreCodedChatbot
{
    public class Constants
    {
        public const double MsgRate = (20d / 30d);
        public static Regex ChatMsg = new Regex(@"^:\w+!\w+@\w+\.tmi\.twitch\.tv PRIVMSG #\w+ :");
    }
}
