using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.Models;
using DVSRegister.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DVSAdmin.Controllers
{
   
    public class LoginController : Controller
    {
        private readonly ISignUpService _signUpService;

        public LoginController(ISignUpService signUpService)
        {
            _signUpService = signUpService;
        }

        [HttpGet("")]
        public IActionResult SignUp()
        {
            return View("SignUp");
        }

        [HttpPost("create-new-account")]
        public async Task<IActionResult> CreateNewAccount(SignUpViewModel signUpViewModel)
        {

            if (ModelState["Email"].Errors.Count == 0)
            {
                var forgotPasswordResponse = await _signUpService.ForgotPassword(signUpViewModel.Email);

                if (forgotPasswordResponse == "OK")
                {
                    HttpContext.Session?.Set("Email", signUpViewModel.Email);
                    return RedirectToAction("ConfirmPassword", "Login");
                }
                else
                {
                    return View("SignUp");
                }
            }

            else
            {
                return View("SignUp");
            }
        }

        [HttpGet("enter-email-address")]
        public IActionResult ConfirmPassword()
        {
            ConfirmPasswordViewModel confirmPasswordViewModel = new ConfirmPasswordViewModel();
            confirmPasswordViewModel.Email = HttpContext?.Session.Get<string>("Email");
            return View("ConfirmPassword", confirmPasswordViewModel);
        }

        [HttpPost("confirm-password")]
        public async Task<IActionResult> ConfirmPasswordCheck(ConfirmPasswordViewModel confirmPasswordViewModel)
        {
            confirmPasswordViewModel.Email = HttpContext?.Session.Get<string>("Email");
            if (ModelState["Password"].Errors.Count ==0 && ModelState["ConfirmPassword"].Errors.Count==0)
            {
                var confirmPasswordResponse = await _signUpService.ConfirmPassword(confirmPasswordViewModel.Email, confirmPasswordViewModel.Password, confirmPasswordViewModel.OneTimePassword);
                if (confirmPasswordResponse.Length > 0)
                {
                    HttpContext?.Session.Set("MFARegistrationViewModel", new MFARegistrationViewModel { Email = confirmPasswordViewModel.Email, Password = confirmPasswordViewModel.Password, SecretToken = confirmPasswordResponse });
                    return RedirectToAction("MFARegistration", "Login");
                }
                else
                {
                    return RedirectToAction(Constants.ErrorPath);
                }
            }
            else
            {
                return View("ConfirmPassword", confirmPasswordViewModel);
            }
                        
        }

        [HttpGet("mfa-registration")]
        public IActionResult MFARegistration()
        {
            MFARegistrationViewModel MFARegistrationViewModel = HttpContext?.Session.Get<MFARegistrationViewModel>("MFARegistrationViewModel");
            return View("MFARegistration", MFARegistrationViewModel);
        }

        [HttpPost("mfa-confirmation")]
        public async Task<IActionResult> MFAConfirmationCheck(MFARegistrationViewModel viewModel)
        {
            MFARegistrationViewModel MFARegistrationViewModel = HttpContext?.Session.Get<MFARegistrationViewModel>("MFARegistrationViewModel");
            if (ModelState["MFACode"].Errors.Count == 0)
            {
                var mfaConfirmationCheckResponse = await _signUpService.MFAConfirmation(MFARegistrationViewModel.Email, MFARegistrationViewModel.Password, viewModel.MFACode);

                if (mfaConfirmationCheckResponse == "OK")
                {
                    return RedirectToAction("LoginPage");
                }
                else
                {
                    return RedirectToAction(Constants.ErrorPath);
                }
            }
            else
            {
                return View("MFARegistration", viewModel);
            }
            
        }

        [HttpGet("login")]
        public IActionResult LoginPage()
        {
            return View("LoginPage");
        }

        [HttpPost("login-to-account")]
        public async Task<IActionResult> LoginToAccount(LoginPageViewModel loginPageViewModel)
        {
            if (ModelState["Email"].Errors.Count == 0 && ModelState["Password"].Errors.Count ==0)
            {
                var loginResponse = await _signUpService.SignInAndWaitForMfa(loginPageViewModel.Email, loginPageViewModel.Password);
                if (loginResponse.Length > 0)
                {
                    HttpContext?.Session.Set("Email", loginPageViewModel.Email);
                    HttpContext?.Session.Set("Session", loginResponse);
                    return RedirectToAction("MFAConfirmation");
                }
                else
                {
                    return RedirectToAction("LoginPage");
                }
            }
            else
            {
                return View("LoginPage");
            }
        }

        [HttpGet("mfa-confirmation")]
        public IActionResult MFAConfirmation()
        {
            return View("MFAConfirmation");
        }

        [HttpPost("mfa-confirmation-login")]
        public async Task<IActionResult> ConfirmMFACodeLogin(LoginPageViewModel loginPageViewModel)
        {
            if (ModelState["MFACode"].Errors.Count == 0)
            {
                var mfaResponse = await _signUpService.ConfirmMFAToken(HttpContext?.Session.Get<string>("Session"), HttpContext?.Session.Get<string>("Email"), loginPageViewModel.MFACode);

                if (mfaResponse.Length > 0)
                {
                    HttpContext?.Session.Set("IdToken", mfaResponse);
                    return RedirectToAction("PreRegAtAGlance", "PreRegistrationReview");
                }
                else
                {
                    return RedirectToAction("MFAConfirmation");
                }
            }
            else
            {
                return View("MFAConfirmation");
            }
        }


        [HttpGet("sign-out")]
        public IActionResult OfDiaSignOut()
        {
            HttpContext?.Session.Clear();
            return RedirectToAction("LoginPage", "Login");
        }

    }
}
