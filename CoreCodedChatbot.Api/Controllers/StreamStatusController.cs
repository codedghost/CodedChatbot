using CoreCodedChatbot.Library.Models.ApiRequest;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Api.Controllers
{
    [Route("api/StreamStatus")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class StreamStatusController : Controller
    {
        public StreamStatusController()
        {
            
        }

        [HttpGet]
        public IActionResult GetStreamStatus()
        {

        }

        [HttpPut]
        public IActionResult PutStreamStatus(PutStreamStatusRequest streamStatusRequest)
        {

        }
    }
}