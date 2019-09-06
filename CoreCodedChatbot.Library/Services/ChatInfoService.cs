using System;
using System.Linq;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using Microsoft.EntityFrameworkCore.Internal;

namespace CoreCodedChatbot.Library.Services
{
    public class ChatInfoService : IChatInfoService
    {
        private readonly IChatbotContextFactory _chatbotContextFactory;

        public ChatInfoService(IChatbotContextFactory chatbotContextFactory)
        {
            _chatbotContextFactory = chatbotContextFactory;
        }

        public string RemoveInfo(string[] aliases)
        {
            try
            {
                using (var context = _chatbotContextFactory.Create())
                {
                    var infoCommands = context.InfoCommandKeywords.Where(ick => aliases.Contains(ick.InfoCommandKeywordText));

                    if (infoCommands.GroupBy(ic => ic.InfoCommandId).Count() != 1) return null;

                    var infoCommandKeywordCount = context.InfoCommandKeywords.Count(ick =>
                        ick.InfoCommandId == infoCommands.First().InfoCommandId);

                    if (infoCommands.Count() > infoCommandKeywordCount) return null;

                    context.InfoCommandKeywords.RemoveRange(infoCommands);

                    if (infoCommands.Count() == infoCommandKeywordCount)
                    {
                        var infoCommand = context.InfoCommands.FirstOrDefault(
                            ic => ic.InfoCommandId == infoCommands.First().InfoCommandId);

                        if (infoCommand == null) return null;

                        context.InfoCommands.Remove(infoCommand);
                        context.SaveChanges();

                        return "I have removed all the aliases you listed and the info command";
                    }

                    var remainingAliases = context.InfoCommandKeywords.Where(ick =>
                        ick.InfoCommandId == infoCommands.First().InfoCommandId).Select(ick => ick.InfoCommandKeywordText).Join();

                    context.SaveChanges();

                    return $"I have removed the aliases you gave me, these are still available: {remainingAliases}";
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"{e} - {e.InnerException}");
            }

            return null;
        }
    }
}
