using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.Models.Home;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("open-tasks")]
    public class HomeController : BaseController
    {
        private readonly IHomeService homeService;
        private readonly IUserService userService;
        private readonly IConfiguration configuration;


        public HomeController(IHomeService homeService, IUserService userService, IConfiguration configuration)
        {
            this.homeService = homeService;
            this.userService = userService;
            this.configuration = configuration;
        }


        [HttpGet("pending-certificate-reviews")]
        public async Task<ActionResult> PendingCertificateReviews(string CurrentSort = "days", string CurrentSortAction = "ascending",
            int pageNumber = 1, string NewSort = "")
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

            var results = await homeService.GetServices(pageNumber, CurrentSort, CurrentSortAction, "cert review");
            var totalPages = (int)Math.Ceiling((double)results.TotalCount / 10);

            OpenTaskCount openTaskCount = new();
            //Fetch pending count from db                
                    
            PendingListViewModel pendingListViewModel = new PendingListViewModel
            {
                PendingRequests = results.Items,
                TotalPages = totalPages,
                CurrentSort = CurrentSort,
                CurrentSortAction = CurrentSortAction,
                OpenTaskCount = openTaskCount,

            };

            ViewBag.CurrentPage = pageNumber;
            return View(pendingListViewModel);
        }

        [HttpGet("pending-primary-checks")]
        public async Task<ActionResult> PendingPrimaryChecks(string CurrentSort = "days", string CurrentSortAction = "ascending",
            int pageNumber = 1, string NewSort = "")
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

            var results = await homeService.GetServices(pageNumber, CurrentSort, CurrentSortAction, "primary check");
            var totalPages = (int)Math.Ceiling((double)results.TotalCount / 10);

            OpenTaskCount openTaskCount = new();
            //Fetch pending count from db                

            PendingListViewModel pendingListViewModel = new PendingListViewModel
            {
                PendingRequests = results.Items,
                TotalPages = totalPages,
                CurrentSort = CurrentSort,
                CurrentSortAction = CurrentSortAction,
                OpenTaskCount = openTaskCount,

            };

            ViewBag.CurrentPage = pageNumber;
            return View(pendingListViewModel);
        }

        [HttpGet("pending-secondary-checks")]
        public async Task<ActionResult> PendingSecondaryChecks(string CurrentSort = "days", string CurrentSortAction = "ascending",
            int pageNumber = 1, string NewSort = "")
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

            var results = await homeService.GetServices(pageNumber, CurrentSort, CurrentSortAction, "secondary check");
            var user = await homeService.GetUserByEmail(UserEmail);
            var totalPages = (int)Math.Ceiling((double)results.TotalCount / 10);

            OpenTaskCount openTaskCount = new();
            //Fetch pending count from db                

            PendingListViewModel pendingListViewModel = new PendingListViewModel
            {
                PendingRequests = results.Items,
                TotalPages = totalPages,
                CurrentSort = CurrentSort,
                CurrentSortAction = CurrentSortAction,
                OpenTaskCount = openTaskCount,

            };
            ViewBag.UserId = user?.Id;
            ViewBag.CurrentPage = pageNumber;
            return View(pendingListViewModel);
        }

        [HttpGet("pending-update-or-removal")]
        public async Task<ActionResult> PendingUpdateOrRemoval(string CurrentSort = "status", string CurrentSortAction = "ascending",
            int pageNumber = 1, string NewSort = "")
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

            var results = await homeService.GetServices(pageNumber, CurrentSort, CurrentSortAction, "update or removal");
            var totalPages = (int)Math.Ceiling((double)results.TotalCount / 10);

            OpenTaskCount openTaskCount = new();
            //Fetch pending count from db                

            PendingListViewModel pendingListViewModel = new PendingListViewModel
            {
                PendingRequests = results.Items,
                TotalPages = totalPages,
                CurrentSort = CurrentSort,
                CurrentSortAction = CurrentSortAction,
                OpenTaskCount = openTaskCount,

            };

            ViewBag.CurrentPage = pageNumber;
            return View(pendingListViewModel);
        }
    }
}
