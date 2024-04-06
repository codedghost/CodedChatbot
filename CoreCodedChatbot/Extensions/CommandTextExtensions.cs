using System.Linq;

namespace CoreCodedChatbot.Extensions
{
    public static class CommandTextExtensions
    {
        public static string[] SplitCommandText(this string commandText, string characterToSplitOn = " ")
        {
            return commandText.Split(characterToSplitOn).Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
        }
    }
}