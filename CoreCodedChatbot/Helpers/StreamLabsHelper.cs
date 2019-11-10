using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CoreCodedChatbot.Config;
using CoreCodedChatbot.Database.Context;
using CoreCodedChatbot.Database.Context.Interfaces;
using CoreCodedChatbot.Database.Context.Models;
using CoreCodedChatbot.Interfaces;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Secrets;
using Newtonsoft.Json;

namespace CoreCodedChatbot.Helpers
{
    public class StreamLabsHelper : IStreamLabsHelper
    {
        private readonly IChatbotContextFactory _chatbotContextFactory;
        private readonly IVipHelper _vipHelper;
        private readonly IConfigService _configService;
        private readonly ISecretService _secretService;

        private HttpClient httpClient = new HttpClient();

        public StreamLabsHelper(IChatbotContextFactory chatbotContextFactory, IVipHelper vipHelper,
            IConfigService configService, ISecretService secretService)
        {
            _chatbotContextFactory = chatbotContextFactory;
            _vipHelper = vipHelper;
            _configService = configService;
            _secretService = secretService;
        }

        public bool RefreshAuthToken()
        {
            try
            {
                using (var context = _chatbotContextFactory.Create())
                {
                    var refreshTokenRecord =
                        context.Settings.SingleOrDefault(s => s.SettingName == "StreamLabsRefreshToken");
                    if (refreshTokenRecord == null) return false;

                    var vals = new Dictionary<string, string>
                    {
                        {"grant_type", "refresh_token"},
                        {"client_id", _secretService.GetSecret<string>("StreamLabsClientId")},
                        {"client_secret", _secretService.GetSecret<string>("StreamLabsClientSecret")},
                        {"redirect_uri", "localhost"},
                        {"refresh_token", refreshTokenRecord.SettingValue}
                    };
                    var content = new FormUrlEncodedContent(vals);
                    var tokenResponse = httpClient.PostAsync("https://streamlabs.com/api/v1.0/token", content).Result;

                    var tokenJsonString = tokenResponse.Content.ReadAsStringAsync().Result;
                    var tokenModel = JsonConvert.DeserializeObject<TokenJsonModel>(tokenJsonString);

                    var accessTokenRecord =
                        context.Settings.SingleOrDefault(s => s.SettingName == "StreamLabsAccessToken") ??
                        new Setting {SettingName = "StreamLabsAccessToken"};

                    accessTokenRecord.SettingValue = tokenModel.access_token;
                    refreshTokenRecord.SettingValue = tokenModel.refresh_token;

                    context.SaveChanges();
                }

                return true;
            }
            catch (Exception)
            {
                Console.Out.WriteLine("Could not refresh Streamlabs token");
                return false;
            }
        }

        private List<StreamLabsDonation> GetRecentDonations()
        {
            using (var context = _chatbotContextFactory.Create())
            {
                try
                {
                    var accessToken = context.Settings.SingleOrDefault(s => s.SettingName == "StreamLabsAccessToken")
                        ?.SettingValue;
                    if (accessToken == null) return null;

                    var lastDonationId = context.Settings.SingleOrDefault(s => s.SettingName == "LatestDonationId")
                                             ?.SettingValue ?? "";

                    var getDonationsResponse = httpClient
                        .GetAsync(
                            $"https://streamlabs.com/api/v1.0/donations?access_token={accessToken}&after={lastDonationId}&currency={_configService.Get<string>("DonationCurrency")}&limit=100")
                        .Result;
                    if (!getDonationsResponse.IsSuccessStatusCode) return null;

                    var donationsJsonString = getDonationsResponse.Content.ReadAsStringAsync().Result;
                    var donationsModel = JsonConvert.DeserializeObject<DonationsModel>(donationsJsonString);

                    return donationsModel.data.ToList();
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public bool CheckDonationVips()
        {
            var attempted = 0;
            var retries = 2;
            List<StreamLabsDonation> donations = null;

            while (donations == null)
            {
                donations = GetRecentDonations();

                if (donations == null) RefreshAuthToken();
                attempted++;

                if (attempted >= retries) break;
            }

            if (donations == null) return false;

            return (!donations.Any() || AlignDb(donations));
        }

        private bool AlignDb(List<StreamLabsDonation> donations)
        {
            try
            {
                // Get most recent donation id
                var latestDonationId = donations.OrderByDescending(d => d.created_at).FirstOrDefault()?.donation_id;
                if (latestDonationId == null) return true;

                // Get all donation amounts together
                var groupedDonations = donations.OrderByDescending(d => d.created_at).ToList()
                    .GroupBy(d => d.name.ToLower()).ToList()
                    .Select(d => new
                    {
                        name = d.First().name.ToLower(),
                        amount = (int) Math.Round(d.Sum(rec => rec.amount) * 100)
                    });

                using (var context = _chatbotContextFactory.Create())
                {
                    var latestDonationIdSetting =
                        context.Settings.FirstOrDefault(s => s.SettingName == "LatestDonationId") ?? new Setting
                        {
                            SettingName = "LatestDonationId"
                        };

                    latestDonationIdSetting.SettingValue = latestDonationId.Value.ToString();

                    foreach (var donation in groupedDonations)
                    {
                        var user = _vipHelper.FindUser(context, donation.name);
                        user.TotalDonated += donation.amount;

                        _vipHelper.GiveDonationVipsDb(user);
                        context.SaveChanges();
                    }

                    context.SaveChanges();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}