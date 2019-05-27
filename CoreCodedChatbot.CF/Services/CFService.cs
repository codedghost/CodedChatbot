using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using CoreCodedChatbot.CF.Interfaces.Services;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Database.Context.Models;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;

namespace CoreCodedChatbot.CF.Services
{
    public class CFService : ICFService
    {
        private IChatbotContextFactory _chatbotContextFactory;
        private ConfigModel _config;

        private Dictionary<int, SongRequest> _pendingDownloadRequests;

        private Timer _pendingJobsTimer;
        private Timer _getDownloadLinkTimer;

        private string _cfCookieAuthHeader = string.Empty;
        private string _communityMemberId = string.Empty;

        private int _recurringJobInterval = 30;
        private int _downloadTimerDelayedStart = 45;

        public CFService(IChatbotContextFactory chatbotContextFactory, IConfigService configService)
        {
            _chatbotContextFactory = chatbotContextFactory;
            _config = configService.GetConfig();

            _pendingDownloadRequests = new Dictionary<int, SongRequest>();
        }

        public bool Main()
        {
            // Auth with CF
            _cfCookieAuthHeader = AuthWithCF();

            if (string.IsNullOrWhiteSpace(_cfCookieAuthHeader)) return false;

            _communityMemberId = _cfCookieAuthHeader.Split(';').FirstOrDefault(c => c.Contains("-community-member_id"))
                ?.Split('=')[1].Trim();

            if (string.IsNullOrWhiteSpace(_communityMemberId)) return false;

            TestSearch();

            // Set up recurring jobs
            _pendingJobsTimer = new Timer(e => CheckPendingJobs(), null, TimeSpan.Zero, TimeSpan.FromSeconds(_recurringJobInterval));

            _getDownloadLinkTimer = new Timer(e => GetDownloadLinks(), null, TimeSpan.FromSeconds(45), TimeSpan.FromSeconds(_recurringJobInterval));

            return true;
        }

        private void TestSearch()
        {
            var data = "draw=2&columns[0][data][_]=19&columns[0][data][display]=undefined&columns[0][name]=&columns[0][searchable]=true&columns[0][orderable]=false&columns[0][search][value]=&columns[0][search][regex]=false&columns[1][data][_]=1&columns[1][data][display]=undefined&columns[1][name]=&columns[1][searchable]=true&columns[1][orderable]=true&columns[1][search][value]=&columns[1][search][regex]=false&columns[2][data][_]=2&columns[2][data][display]=undefined&columns[2][name]=&columns[2][searchable]=true&columns[2][orderable]=true&columns[2][search][value]=&columns[2][search][regex]=false&columns[3][data]=3&columns[3][name]=&columns[3][searchable]=true&columns[3][orderable]=true&columns[3][search][value]=&columns[3][search][regex]=false&columns[4][data][_]=4&columns[4][data][display]=undefined&columns[4][name]=&columns[4][searchable]=true&columns[4][orderable]=true&columns[4][search][value]=&columns[4][search][regex]=false&columns[5][data]=5&columns[5][name]=&columns[5][searchable]=true&columns[5][orderable]=true&columns[5][search][value]=&columns[5][search][regex]=false&columns[6][data]=6&columns[6][name]=&columns[6][searchable]=true&columns[6][orderable]=true&columns[6][search][value]=&columns[6][search][regex]=false&columns[7][data][_]=7&columns[7][data][display]=undefined&columns[7][name]=&columns[7][searchable]=true&columns[7][orderable]=true&columns[7][search][value]=&columns[7][search][regex]=false&columns[8][data][_]=8&columns[8][data][display]=undefined&columns[8][name]=&columns[8][searchable]=true&columns[8][orderable]=true&columns[8][search][value]=&columns[8][search][regex]=false&columns[9][data]=9&columns[9][name]=&columns[9][searchable]=true&columns[9][orderable]=true&columns[9][search][value]=&columns[9][search][regex]=false&columns[10][data][_]=10&columns[10][data][display]=undefined&columns[10][name]=&columns[10][searchable]=true&columns[10][orderable]=true&columns[10][search][value]=&columns[10][search][regex]=false&columns[11][data][_]=11&columns[11][data][filter]=11&columns[11][data][display]=undefined&columns[11][name]=&columns[11][searchable]=true&columns[11][orderable]=true&columns[11][search][value]=&columns[11][search][regex]=false&columns[12][data][_]=12&columns[12][data][display]=undefined&columns[12][name]=&columns[12][searchable]=true&columns[12][orderable]=true&columns[12][search][value]=&columns[12][search][regex]=false&columns[13][data]=13&columns[13][name]=&columns[13][searchable]=true&columns[13][orderable]=true&columns[13][search][value]=&columns[13][search][regex]=false&columns[14][data]=14&columns[14][name]=&columns[14][searchable]=true&columns[14][orderable]=true&columns[14][search][value]=&columns[14][search][regex]=false&columns[15][data]=15&columns[15][name]=&columns[15][searchable]=true&columns[15][orderable]=true&columns[15][search][value]=&columns[15][search][regex]=false&columns[16][data]=16&columns[16][name]=&columns[16][searchable]=true&columns[16][orderable]=true&columns[16][search][value]=&columns[16][search][regex]=false&columns[17][data]=17&columns[17][name]=&columns[17][searchable]=true&columns[17][orderable]=true&columns[17][search][value]=&columns[17][search][regex]=false&columns[18][data]=18&columns[18][name]=&columns[18][searchable]=true&columns[18][orderable]=true&columns[18][search][value]=&columns[18][search][regex]=false&columns[19][data]=19&columns[19][name]=&columns[19][searchable]=true&columns[19][orderable]=true&columns[19][search][value]=&columns[19][search][regex]=false&columns[20][data]=20&columns[20][name]=&columns[20][searchable]=true&columns[20][orderable]=true&columns[20][search][value]=&columns[20][search][regex]=false&order[0][column]=8&order[0][dir]=desc&start=0&length=25&search[value]=death+ascension+resurrectio&search[regex]=false";
            var searchRequest = WebRequest.Create($"http://ignition.customsforge.com/cfss?u={_communityMemberId}");
            searchRequest.Method = WebRequestMethods.Http.Post;
            searchRequest.ContentType = "application/x-www-form-urlencoded";
            searchRequest.ContentLength = data.Length;
            searchRequest.Headers.Add("Origin", "http://ignition.customsforge.com");
            searchRequest.Headers.Add("Referer", "http://ignition.customsforge.com");
            searchRequest.Headers.Add("Cookie", _cfCookieAuthHeader);

            var searchStream = new StreamWriter(searchRequest.GetRequestStream());
            searchStream.Write(data);
            searchStream.Flush();
            searchStream.Close();

            var response = searchRequest.GetResponse();

            var responseStream = response.GetResponseStream();
            if (responseStream == null) return;

            var responseStreamReader = new StreamReader(responseStream);

            var responseString = responseStreamReader.ReadToEnd();
        }

        private string AuthWithCF()
        {
            var queryString = string.Format(_config.CFAuthLink, _config.CFUsername, _config.CFPassword);

            try
            {
                var request = (HttpWebRequest) WebRequest.Create(_config.CFLoginLink);
                request.Method = WebRequestMethods.Http.Post;
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = queryString.Length;

                var requestStream = new StreamWriter(request.GetRequestStream());
                requestStream.Write(queryString);
                requestStream.Flush();
                requestStream.Close();

                var response = (HttpWebResponse) request.GetResponse();
                var responseStream = response.GetResponseStream();

                if (responseStream == null) return string.Empty;
                var responseStreamReader = new StreamReader(responseStream);
                var res = responseStreamReader.ReadToEnd();

                var authCookieHeader = response.Headers.Get("Set-Cookie");

                response.Close();
                return !res.Contains("Username or password incorrect.") ? authCookieHeader : string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex} - {ex.InnerException}");
                return string.Empty;
            }
        }

        private void CheckPendingJobs()
        {
            using (var context = _chatbotContextFactory.Create())
            {
                var pendingRequests = context.SongRequests.Where(sr => !sr.Played && !sr.InDrive);

                foreach (var request in pendingRequests)
                {
                    _pendingDownloadRequests.TryAdd(request.SongRequestId, request);
                }
            }
        }

        private void GetDownloadLinks()
        {

        }
    }
}
