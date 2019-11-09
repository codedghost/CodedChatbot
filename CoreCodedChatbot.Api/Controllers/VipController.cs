using System;
using CoreCodedChatbot.Library.Interfaces.Services;
using CoreCodedChatbot.Library.Models.ApiRequest.Vip;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Api.Controllers
{
    [Route("Vip/[action]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class VipController : Controller
    {
        private IVipService _vipService;

        public VipController(IVipService vipService)
        {
            _vipService = vipService;
        }

        [HttpPost]
        public IActionResult GiftVip([FromBody] GiftVipModel giftVipModel)
        {
            try
            {
                if (_vipService.GiftVip(giftVipModel.DonorUsername, giftVipModel.ReceiverUsername)) return Ok();
            }
            catch (Exception e)
            {
                Console.Out.Write($"{e} - {e.InnerException}");
            }

            return BadRequest();
        }

        [HttpPost]
        public IActionResult ModGiveVip([FromBody] ModGiveVipModel modGiveVipModel)
        {
            try
            {
                if (_vipService.ModGiveVip(modGiveVipModel.ReceivingUsername, modGiveVipModel.VipsToGive)) return Ok();
            }
            catch (Exception e)
            {
                Console.Error.Write($"ModGiveVIP\nException:\n{e}\nInner:\n{e.InnerException}");
            }

            return BadRequest();
        }
    }
}
