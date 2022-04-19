using System.Linq;

namespace CoreCodedChatbot.Extensions
{
    public static class CommandTextExtensions
    {
        public static string[] SplitCommandText(this string commandText)
        {
            return commandText.Split(" ").Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
        }
    }
}