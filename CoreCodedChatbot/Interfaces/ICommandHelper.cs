using System;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Interfaces
{
    public interface ICommandHelper
    {
        void Init(IServiceProvider serviceProvider);

        Task ProcessCommand(string userCommand, TwitchClient client, string username, string userParameters,
            bool userIsModOrBroadcaster, JoinedChannel joinedRoom);

    }
}