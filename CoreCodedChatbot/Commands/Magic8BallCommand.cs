using System;
using System.Threading.Tasks;
using CoreCodedChatbot.Constants;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "8ball", "magicball", "magic8ball", "crystalball" }, false)]
    public class Magic8BallCommand : ICommand
    {
        private Random _rand;

        public Magic8BallCommand()
        {
            _rand = new Random();
        }

        public async Task Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            if (string.IsNullOrWhiteSpace(commandText))
            {
                client.SendMessage(joinedChannel, $"Hey @{username}, you need to ask a question!");
                return;
            }

            var response = Magic8BallResponses.Responses[_rand.Next(Magic8BallResponses.Responses.Count)];

            client.SendMessage(joinedChannel, $"Hey @{username}, you asked: {commandText} - {response}");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command lets you ask questions to the magic 8 ball!");
        }
    }
}