using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.Models;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{

    [Route("public-interest-check")]
    [ValidCognitoToken]
    public class PublicInterestCheckController : Controller
    {
      
        private readonly IPublicInterestCheckService publicInterestCheckService;
       
        public PublicInterestCheckController(IPublicInterestCheckService publicInterestCheckService)
        {           
            this.publicInterestCheckService = publicInterestCheckService;            
        }

        [HttpGet("public-interest-check-list")]
        public async Task<IActionResult> PublicInterestCheck()
        {
        
        PublicInterestCheckViewModel publicInterestCheckViewModel = new PublicInterestCheckViewModel();
        var publicinterestchecks = await publicInterestCheckService.GetPICheckList();
        return View(publicInterestCheckViewModel);          

        }
    }
}