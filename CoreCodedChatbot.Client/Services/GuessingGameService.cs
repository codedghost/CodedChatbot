using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.ApiRequest.GuessingGame;
using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Client.Services
{
    public class GuessingGameService : Interfaces.IGuessingGameService
    {
        private readonly IConfigService _configService;
        private readonly IGuessingGameApiClient _guessingGameApiClient;
        private Timer _checkFileTimer;

        private string _rocksnifferDirectory;

        private bool hasGameStarted = false;
        private bool hasGameBeenCompleted = false;

        private int totalTime = 0;

        public GuessingGameService(IConfigService configService, IGuessingGameApiClient guessingGameApiClient)
        {
            _configService = configService;
            _guessingGameApiClient = guessingGameApiClient;

            _rocksnifferDirectory = _configService.Get<string>("RocksnifferSongDetailsLocation");

            // Using a polling model rather than FileWatcher.
            // File watcher is triggered a lot due to the file being continually written to.
            _checkFileTimer = new Timer(async x => await CheckRocksnifferFiles(),
                null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1));
        }

        private async Task CheckRocksnifferFiles()
        {

            var songDetailsLocation = _rocksnifferDirectory + "song_details.txt";
            var songTimerLocation = _rocksnifferDirectory + "song_timer.txt"; // split on / left is current time, right is total.
            var songAccuracyLocation = _rocksnifferDirectory + "accuracy.txt";

            //TempWriteToFile($"SongDetails: {GetFileContents(songDetailsLocation)}");
            //TempWriteToFile($"SongTimer: {GetFileContents(songTimerLocation)}");
            //TempWriteToFile($"SongAccuracy: {GetFileContents(songAccuracyLocation)}");

            var timerText = GetFileContents(songTimerLocation);
            var songName = GetFileContents(songDetailsLocation);
            var finalPercentage = GetFileContents(songAccuracyLocation).Trim('%');

            if (string.IsNullOrWhiteSpace(timerText)) return;

            var timer = timerText.Split("/");

            var runningTimeInSeconds = ConvertTimerToSeconds(timer[0]);

            var songInfoModel = new StartGuessingGameModel
            {
                SongName = songName,
                SongLengthSeconds = ConvertTimerToSeconds(timer[1])
            };

            if (runningTimeInSeconds != 0)
            {
                if (!hasGameStarted && !hasGameBeenCompleted)
                {
                    // send request to start guessing game
                    hasGameStarted = true;
                    hasGameBeenCompleted = false;

                    var success = await _guessingGameApiClient.StartGuessingGame(songInfoModel);

                    if (!success) return;

                    totalTime = ConvertTimerToSeconds(timer[1]);
                    return;
                }

                // Need hasGameBeenCompleted flag as the file remains on full time for a few seconds, allowing us to grab the final score.
                if (runningTimeInSeconds != totalTime || hasGameBeenCompleted) return;

                // send percentage to server and finish game
                decimal.TryParse(finalPercentage, out var finalPercentageDecimal);
                var completedResult = await _guessingGameApiClient.FinishGuessingGame(finalPercentageDecimal);

                if (!completedResult) return;

                hasGameStarted = false;
                hasGameBeenCompleted = true;
                return;
            }

            hasGameStarted = false;
            hasGameBeenCompleted = false;
        }

        private static int ConvertTimerToSeconds(string timerText)
        {
            var minutesAndSeconds = timerText.Split(":");
            return int.Parse(minutesAndSeconds[0]) * 60 + int.Parse(minutesAndSeconds[1]);
        }

        private string GetFileContents(string fileLocation)
        {
            try
            {
                using (var sr = new StreamReader(File.OpenRead(fileLocation)))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
