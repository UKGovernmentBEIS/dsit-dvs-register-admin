using Microsoft.AspNetCore.Mvc;
using DVSRegister.Extensions;
using DVSAdmin.BusinessLogic.Services;
using System.Security.Claims;

namespace DVSAdmin.Controllers
{
    [ValidCognitoToken]
    public class DigitalIdentityController : Controller
    {
        private readonly IUserService userService;
        private string userEmail => HttpContext.Session.Get<string>("Email") ?? string.Empty;
        private string userprofile => HttpContext.Session.Get<string>("Profile") ?? string.Empty;
        public DigitalIdentityController(IUserService userService)
        {
            this.userService = userService;

        }
        [Route("home")]
        public async Task<IActionResult> LandingPage()
        {
            if(string.IsNullOrEmpty(userprofile))
            {
                string profile = string.Empty;
                var identity = HttpContext?.User.Identity as ClaimsIdentity;
                var profileClaim = identity?.Claims.FirstOrDefault(c => c.Type == "profile");
                if (profileClaim != null)
                {
                    profile = profileClaim.Value;
                    HttpContext?.Session.Set("Profile", profile);
                }
                await userService.UpdateUserProfile(userEmail, profile);
            }
           
            return View();
        }
    }
}
