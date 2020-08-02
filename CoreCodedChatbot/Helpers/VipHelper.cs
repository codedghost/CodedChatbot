using CoreCodedChatbot.ApiContract.Enums.VIP;
using TwitchLib.PubSub.Enums;
using TwitchLib.PubSub.Events;

namespace CoreCodedChatbot.Helpers
{
    public static class VipHelper
    {
        public static SubscriptionTier GetSubTier(OnChannelSubscriptionArgs channelSubArgs)
        {
            switch (channelSubArgs.Subscription.SubscriptionPlan)
            {
                case SubscriptionPlan.Prime:
                    return SubscriptionTier.Prime;
                case SubscriptionPlan.Tier1:
                    return SubscriptionTier.Tier1;
                case SubscriptionPlan.Tier2:
                    return SubscriptionTier.Tier2;
                case SubscriptionPlan.Tier3:
                    return SubscriptionTier.Tier3;
                default:
                    // assuming prime if no data
                    return SubscriptionTier.Prime;
            }
        }
    }
}