using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [Route("register-management")]
    public class RegisterManagementController : Controller
    {
        [HttpGet("register-management-list")]
        public IActionResult RegisterManagement()
        {
            return View();
        }
    }
}
