using DVSAdmin.BusinessLogic.Models;
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
        public async Task<IActionResult> AllPublishedServices(int pageNumber = 1, string SearchText = "", string SearchAction = "")
        {
            if(SearchAction == "clearSearch")
            {
                ModelState.Clear();
                SearchText = string.Empty;
            }
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

        [HttpGet("reassign-service")]
        public async Task<IActionResult> ServiceReassign(int serviceId, bool isTransferOngoing)
        {
            if (isTransferOngoing)
            {
                CabTransferRequestDto cabTransferRequestDto = await cabTransferService.GetCabTransferDetails(serviceId);
                ServiceDto service = cabTransferRequestDto.Service;
                ViewBag.ToCabName = cabTransferRequestDto.ToCab.CabName;
                return View(service);
            }
            else
            {
                ServiceDto service = await cabTransferService.GetServiceDetails(serviceId);  
                return View(service);
            }
           
        }

        [HttpGet("service-reassign-start")]
        public async Task<IActionResult> ReassignServiceToCAB(int serviceId)
        {
            ServiceDto service = await cabTransferService.GetServiceDetails(serviceId);
            return View(service);
        }

        [HttpGet("cancel-reassign")]
        public async Task<IActionResult> CancelAssignmentRequest(int serviceId)
        {
            CabTransferRequestDto cabTransferRequestDto = await cabTransferService.GetCabTransferDetails(serviceId);
            return View(cabTransferRequestDto);
        }

    }
}
