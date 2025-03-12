using Microsoft.AspNetCore.Mvc;
using DVSRegister.Extensions;
using DVSAdmin.BusinessLogic.Services;
using System.Security.Claims;

namespace DVSAdmin.Controllers
{
    public class DigitalIdentityController : BaseController
    {
        private readonly IUserService userService;           
        public DigitalIdentityController(IUserService userService)
        {
            this.userService = userService;

        }
        [Route("home")]
        public async Task<IActionResult> LandingPage()
        {
            if(string.IsNullOrEmpty(UserProfile))
            {
                string profile = string.Empty;
                var identity = HttpContext?.User.Identity as ClaimsIdentity;
                var profileClaim = identity?.Claims.FirstOrDefault(c => c.Type == "profile");
                if (profileClaim != null)
                {
                    profile = profileClaim.Value;
                    HttpContext?.Session.Set("Profile", profile);
                }
                await userService.UpdateUserProfile(UserEmail, profile);
            }

            return View();
        }
    }
}
