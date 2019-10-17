using System.Threading.Tasks;
using CoreCodedChatbot.ApiClient.Interfaces.ApiClients;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.ApiRequest.StreamStatus;
using CoreCodedChatbot.Library.Models.Data;
using CoreCodedChatbot.Library.Models.View.ComponentViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Components
{
    public class NavViewComponent : ViewComponent
    {
        private readonly IStreamStatusClient _streamStatusClient;
        private ConfigModel _config;

        public NavViewComponent(IStreamStatusClient streamStatusClient, IConfigService configService)
        {
            _streamStatusClient = streamStatusClient;

            _config = configService.GetConfig();
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var streamStatus = await _streamStatusClient.GetStreamStatus(
                new GetStreamStatusRequest
                {
                    BroadcasterUsername = _config.StreamerChannel
                });
            
            return View(new NavigationViewModel
            {
                IsBroadcasterOnline = streamStatus.IsOnline
            });
        }
    }
}