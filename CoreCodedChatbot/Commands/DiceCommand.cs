using System;
using CoreCodedChatbot.Interfaces;
using TwitchLib.Client;
using TwitchLib.Client.Models;

namespace CoreCodedChatbot.Commands
{
    [CustomAttributes.ChatCommand(new[] { "dice", "d", "die" }, false)]
    public class DiceCommand : ICommand
    {
        private Random _rand;

        public DiceCommand()
        {
            _rand = new Random();
        }

        public void Process(TwitchClient client, string username, string commandText, bool isMod, JoinedChannel joinedChannel)
        {
            if (int.TryParse(commandText, out var diceSides) && diceSides > 1 && diceSides <= 100)
            {
                var result = _rand.Next(1, diceSides);

                client.SendMessage(joinedChannel,
                    result == diceSides
                        ? $"Hey @{username}, you rolled a nat {result}! Fortune smiles upon you :)"
                        : result == 1
                            ? $"Hey @{username}, you rolled a crit fail of {result}! Hope that wasn't important!"
                            : $"Hey @{username}, you rolled a {result}");
                return;
            }

            client.SendMessage(joinedChannel, $"Hey @{username}, please pass a number between 2 and 100 e.g: !dice 20");
        }

        public void ShowHelp(TwitchClient client, string username, JoinedChannel joinedChannel)
        {
            client.SendMessage(joinedChannel,
                $"Hey @{username}, this command lets you roll a die, just pass a number between 2 and 100 and it'll give you the result!");
        }
    }
}