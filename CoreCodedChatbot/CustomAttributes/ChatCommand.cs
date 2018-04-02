using System;

namespace CoreCodedChatbot.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class ChatCommand : Attribute
    {
        /*
         * Credit to Curt Morton for Attribute approach
         * Makes adding and editing new Chat Commands super easy
         */

        public string[] CommandAliases { get; }
        public bool ModOnly { get; }

        public ChatCommand(string[] commandAliases, bool modOnly)
        {
            CommandAliases = commandAliases;
            ModOnly = modOnly;
        }
    }
}
