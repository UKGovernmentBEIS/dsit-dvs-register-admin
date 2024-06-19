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
        [HttpGet("provider-review")]
        public IActionResult ProviderReview()
        {
            return View();
        }
        [HttpGet("service-details")]
        public IActionResult ServiceDetails()
        {
            return View();
        }
        [HttpGet("publish-service")]
        public IActionResult PublishService()
        {
            return View();
        }
        [HttpGet("what-next")]
        public IActionResult WhatNext()
        {
            return View();
        }
        [HttpGet("provider-published")]
        public IActionResult ProviderPublished()
        {
            return View();
        }
    }
}
