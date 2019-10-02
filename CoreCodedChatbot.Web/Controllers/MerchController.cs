using System.Collections.Generic;
using System.Threading.Tasks;
using CoreCodedChatbot.Printful.Interfaces.ExternalClients;
using CoreCodedChatbot.Printful.Interfaces.Factories;
using CoreCodedChatbot.Printful.Models.ApiResponse;
using CoreCodedChatbot.Web.ViewModels.Merch;
using Microsoft.AspNetCore.Mvc;

namespace CoreCodedChatbot.Web.Controllers
{
    public class MerchController : Controller
    {
        private readonly IPrintfulClient _printfulClient;

        public MerchController(IPrintfulClientFactory printfulClientFactory)
        {
            _printfulClient = printfulClientFactory.Get();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _printfulClient.GetAllProducts();

            return View("Merch", BuildViewModel(products, string.Empty));
        }

        [HttpPost("/search")]
        public async Task<IActionResult> Search(MerchLandingViewModel submittedModel)
        {
            var products = string.IsNullOrWhiteSpace(submittedModel.SearchTerms)
                ? await _printfulClient.GetAllProducts()
                : await _printfulClient.GetRelevantProducts(submittedModel.SearchTerms);

            return View("Merch", BuildViewModel(products, submittedModel.SearchTerms));
        }

        private MerchLandingViewModel BuildViewModel(List<GetSyncVariantsResult> getSyncProductsResult, string searchTerms)
        {
            return new MerchLandingViewModel
            {
                SearchTerms = searchTerms,
                SyncVariants = getSyncProductsResult
            };
        }
    }
}