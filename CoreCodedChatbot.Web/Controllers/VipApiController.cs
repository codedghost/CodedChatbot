using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.ApiRequest.Vip;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class VipApiController : Controller
    {
        private IVipService vipService;

        public VipApiController(IVipService vipService)
        {
            this.vipService = vipService;
        }

        [HttpPost]
        public IActionResult GiftVip([FromBody] GiftVipModel giftVipModel)
        {
            try
            {
                if (vipService.GiftVip(giftVipModel.DonorUsername, giftVipModel.ReceiverUsername)) return Ok();
            }
            catch (Exception e)
            {
                Console.Out.Write($"{e} - {e.InnerException}");
            }

            return BadRequest();
        }
    }
}
