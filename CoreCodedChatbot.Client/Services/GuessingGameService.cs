using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using CoreCodedChatbot.Library.Helpers;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.Client.Services
{
    public class GuessingGameService
    {
        private ConfigModel config;
        private Timer checkFileTimer;

        private HttpClient guessingGameClient;

        private bool hasGameStarted = false;
        private bool hasGameBeenCompleted = false;

        private int totalTime = 0;

        public GuessingGameService(IConfigService configService)
        {
            config = configService.GetConfig();

            this.guessingGameClient = new HttpClient
            {
                BaseAddress = new Uri(config.GuessingGameApiUrl),
                DefaultRequestHeaders =
                {
                    Authorization = new AuthenticationHeaderValue("Bearer", config.JwtTokenString)
                }
            };

            // Using a polling model rather than FileWatcher.
            // File watcher is triggered a lot due to the file being continually written to.
            checkFileTimer = new Timer(async x => await CheckRocksnifferFiles(),
                null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(2));
        }

        private async Task CheckRocksnifferFiles()
        {

            var songDetailsLocation = config.RocksnifferSongDetailsLocation + "song_details.txt";
            var songTimerLocation =
                config.RocksnifferSongDetailsLocation +
                "song_timer.txt"; // split on / left is current time, right is total.
            var songAccuracyLocation = config.RocksnifferSongDetailsLocation + "accuracy.txt";

            TempWriteToFile($"SongDetails: {GetFileContents(songDetailsLocation)}");
            TempWriteToFile($"SongTimer: {GetFileContents(songTimerLocation)}");
            TempWriteToFile($"SongAccuracy: {GetFileContents(songAccuracyLocation)}");

            var timerText = GetFileContents(songTimerLocation);
            var songName = GetFileContents(songDetailsLocation);
            var finalPercentage = GetFileContents(songAccuracyLocation).Trim('%');

            if (string.IsNullOrWhiteSpace(timerText)) return;

            var timer = timerText.Split("/");

            var runningTimeInSeconds = ConvertTimerToSeconds(timer[0]);

            if (runningTimeInSeconds != 0)
            {
                if (!hasGameStarted && !hasGameBeenCompleted)
                {
                    // send request to start guessing game
                    hasGameStarted = true;
                    hasGameBeenCompleted = false;

                    var result = await guessingGameClient.PostAsync("StartGuessingGame", HttpClientHelper.GetJsonData(songName));

                    if (result.IsSuccessStatusCode)
                    {
                        totalTime = ConvertTimerToSeconds(timer[1]);
                        return;
                    }

                    //OnFail
                    hasGameStarted = false;

                    return;
                }

                // Need hasGameBeenCompleted flag as the file remains on full time for a few seconds, allowing us to grab the final score.
                if (runningTimeInSeconds != totalTime || hasGameBeenCompleted) return;

                // send percentage to server and finish game
                var completedResult = await guessingGameClient.PostAsync("FinishGuessingGame",
                    HttpClientHelper.GetJsonData(finalPercentage));

                if (!completedResult.IsSuccessStatusCode) return;

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
            
        private void TempWriteToFile(string textToAppend)
        {
            var tempLocation = config.RocksnifferSongDetailsLocation + "combo_output_text.txt";
            try
            {
                using (var sw = new StreamWriter(File.Open(tempLocation, FileMode.Append)))
                {
                    sw.WriteLine(textToAppend);
                }
            }
            catch (Exception)
            {
                // File probably in use, pass over
            }
        }
    }
}
