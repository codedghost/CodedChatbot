using System;
using System.Collections.Generic;
using System.Text;

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
