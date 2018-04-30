namespace CoreCodedChatbot.Models.Data
{
    public class CommandClassModel
    {
        public string ClassName { get; set; }
        public bool NeedMod { get; set; }

        public CommandClassModel(string className, bool needMod)
        {
            ClassName = className;
            NeedMod = needMod;
        }
    }
}
