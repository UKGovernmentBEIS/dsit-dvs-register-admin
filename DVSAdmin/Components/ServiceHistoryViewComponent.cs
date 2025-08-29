using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.Models.RegManagement;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Components
{
    public class ServiceHistoryViewComponent : ViewComponent
    {

        private readonly IRegManagementService _regManagementService;

        public ServiceHistoryViewComponent(IRegManagementService regManagementService)
        {
            _regManagementService = regManagementService;
        }

        public async Task<IViewComponentResult> InvokeAsync( int serviceKey, int pageNumber = 1, string CurrentSort  = "status", string CurrentSortAction = "ascending", string NewSort = "")
        {
            if (NewSort != string.Empty)
            {
                if (CurrentSort == NewSort)
                {
                    CurrentSortAction = CurrentSortAction == "ascending" ? "descending" : "ascending";
                }
                else
                {
                    CurrentSort = NewSort;
                    CurrentSortAction = "ascending";
                }
                pageNumber = 1;
            }
            var results = await _regManagementService.GetServiceHistory(pageNumber, CurrentSort, CurrentSortAction);

            var model = new ServiceHistoryViewModel
            {
                Services = results.Items,
                TotalPages = (int)Math.Ceiling((double)results.TotalCount / 10),
                PageNumber = pageNumber,
                CurrentSort = CurrentSort,
                CurrentSortAction = CurrentSortAction,
                ServiceKey = serviceKey
                
            };
            ViewData["ServiceKey"] = serviceKey;
            ViewBag.CurrentPage = pageNumber;
            return View("~/Views/RegisterManagement/Components/ServiceHistory.cshtml", model);
        }

    }
}
