﻿using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
    [ValidCognitoToken]
    public class DigitalIdentityController : Controller
    {
        [Route("home")]
        public IActionResult LandingPage()
        {          
            return View();
        }
    }
}
