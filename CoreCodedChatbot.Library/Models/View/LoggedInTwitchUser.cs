﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Library.Models.View
{
    public class LoggedInTwitchUser
    {
        public bool IsMod { get; set; }
        public string Username { get; set; }
    }
}
