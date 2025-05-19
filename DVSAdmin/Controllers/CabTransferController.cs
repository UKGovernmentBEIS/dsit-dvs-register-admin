using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.Models.CabTransfer;
using Microsoft.AspNetCore.Mvc;


namespace DVSAdmin.Controllers
{
    [Route("cab-transfer")]
    public class CabTransferController: BaseController
    {
        private readonly ICabTransferService cabTransferService;
        private readonly IUserService userService;
        private readonly IConfiguration configuration;

        public CabTransferController(ICabTransferService cabTransferService, IUserService userService, IConfiguration configuration)
        {
            this.cabTransferService = cabTransferService;
            this.userService = userService;
            this.configuration = configuration;
        }

        [HttpGet("published-service-list")]
        public async Task<IActionResult> AllPublishedServices(int pageNumber = 1, string SearchText = "")
        {
            var results = await cabTransferService.GetServices(pageNumber, SearchText);
            var totalPages = (int)Math.Ceiling((double)results.TotalCount / 10);

            ViewBag.CurrentPage = pageNumber;
            var serviceListViewModel = new ServiceListViewModel
            {
                Services = results.Items,
                TotalPages = totalPages
            };
            return View(serviceListViewModel);
        }
    }
}
