namespace CoreCodedChatbot.Library.Models.Enums
{
    public enum ProcessEditArgsResult
    {
        RegularRequest,
        VipRequestNoIndex,
        VipRequestWithIndex,
        OneRequestEdit,
        ArgumentError,
        NoRequestProvided,
        NoRequestInList
    }
}
