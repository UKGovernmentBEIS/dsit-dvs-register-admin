using DVSAdmin.BusinessLogic.Services;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Models;
using DVSRegister.Extensions;
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
        public IActionResult StartPage()
        {
            return View("StartPage");
        }

        [HttpGet("enter-email")]
        public IActionResult EnterEmail(bool passwordReset)
        {
            if (passwordReset)
            {
                EnterEmailViewModel enterEmailViewModel = new EnterEmailViewModel();
                enterEmailViewModel.PasswordReset = passwordReset;
                return View("EnterEmail", enterEmailViewModel);
            }
            return View("EnterEmail");
        }

        [HttpPost("create-new-account")]
        public async Task<IActionResult> CreateNewAccount(EnterEmailViewModel enterEmailViewModel)
        {

            if (ModelState["Email"].Errors.Count == 0)
            {
                var forgotPasswordResponse = await _signUpService.ForgotPassword(enterEmailViewModel.Email);

                if (forgotPasswordResponse == "OK")
                {
                    HttpContext.Session?.Set("Email", enterEmailViewModel.Email);
                    return RedirectToAction("ConfirmPassword", "Login", new { passwordReset = enterEmailViewModel.PasswordReset });
                }
                else
                {
                    ModelState.AddModelError("Email", forgotPasswordResponse);
                    return View("EnterEmail");
                }
            }

            else
            {
                return View("EnterEmail");
            }
        }


        [HttpGet("confirm-password")]
        public IActionResult ConfirmPassword(bool passwordReset)
        {
            ConfirmPasswordViewModel confirmPasswordViewModel = new ConfirmPasswordViewModel();
            confirmPasswordViewModel.PasswordReset = passwordReset;
            string email = HttpContext?.Session.Get<string>("Email");
            ViewBag.Email= email;
            return View("ConfirmPassword", confirmPasswordViewModel);
        }

        [HttpPost("confirm-password")]
        public async Task<IActionResult> ConfirmPasswordCheck(ConfirmPasswordViewModel confirmPasswordViewModel)
        {
            string email = HttpContext?.Session.Get<string>("Email");
            if (ModelState.IsValid)
            {
                GenericResponse confirmPasswordResponse = new GenericResponse();
                if(confirmPasswordViewModel.PasswordReset!= null && confirmPasswordViewModel.PasswordReset ==true)
                {
                    confirmPasswordResponse = await _signUpService.ResetPassword(email, confirmPasswordViewModel.Password, confirmPasswordViewModel.OneTimePassword);
                    if (confirmPasswordResponse.Success)
                    {
                        return View("PasswordChanged");
                    }
                    else
                    {
                        ModelState.AddModelError("ErrorMessage", "Error in resetting password");
                        return View("ConfirmPassword", confirmPasswordViewModel);
                    }
                }
                else
                {
                    confirmPasswordResponse = await _signUpService.ConfirmPassword(email, confirmPasswordViewModel.Password, confirmPasswordViewModel.OneTimePassword);
                    if (confirmPasswordResponse.Success)
                    {
                        HttpContext?.Session.Set("MFARegistrationViewModel", new MFARegistrationViewModel { Email = email, Password = confirmPasswordViewModel.Password, SecretToken = confirmPasswordResponse.Data });
                        return RedirectToAction("MFADescription", "Login");
                    }
                    else
                    {
                        if(confirmPasswordResponse.ErrorMessage == Constants.InvalidCode)
                        {
                            ModelState.AddModelError("OneTimePassword", confirmPasswordResponse.ErrorMessage);
                        }
                        else
                        {
                            ModelState.AddModelError("ErrorMessage", confirmPasswordResponse.ErrorMessage);
                        }

                        return View("ConfirmPassword", confirmPasswordViewModel);
                    }
                }

            }
            else
            {
                return View("ConfirmPassword", confirmPasswordViewModel);
            }

        }

        [HttpGet("mfa-description")]
        public IActionResult MFADescription()
        {           
            return View();
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
                    return View("SignUpComplete");
                }
                else
                {
                    viewModel.SecretToken=MFARegistrationViewModel.SecretToken;
                    viewModel.Email = MFARegistrationViewModel.Email;
                    ModelState.AddModelError("MFACode", "Invalid MFA code provided");
                    return View("MFARegistration", viewModel);
                }
            }
            else
            {
                viewModel.SecretToken=MFARegistrationViewModel.SecretToken;
                viewModel.Email = MFARegistrationViewModel.Email;
                return View("MFARegistration", viewModel);
            }

        }

        [HttpGet("login")]
        public IActionResult LoginPage()
        {
            return View("LoginPage");
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginToAccount(LoginViewModel loginPageViewModel)
        {
            if (ModelState["Email"].Errors.Count == 0 && ModelState["Password"].Errors.Count ==0)
            {
                var loginResponse = await _signUpService.SignInAndWaitForMfa(loginPageViewModel.Email, loginPageViewModel.Password);
                if (loginResponse.Length > 0 && loginResponse != Constants.IncorrectPassword)
                {
                    HttpContext?.Session.Set("Email", loginPageViewModel.Email);
                    HttpContext?.Session.Set("Session", loginResponse);
                    return RedirectToAction("MFAConfirmation");
                }
                else
                {
                    if (loginResponse == Constants.IncorrectPassword)
                    {
                        ModelState.AddModelError("Password", Constants.IncorrectLoginDetails);
                    }
                    else
                    {
                        ModelState.AddModelError("Email", Constants.IncorrectLoginDetails);
                    }
                    return View("LoginPage");
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
        public async Task<IActionResult> ConfirmMFACodeLogin(MFACodeViewModel MFACodeViewModel)
        {
            if (ModelState["MFACode"].Errors.Count == 0)
            {
                var mfaResponse = await _signUpService.ConfirmMFAToken(HttpContext?.Session.Get<string>("Session"), HttpContext?.Session.Get<string>("Email"), MFACodeViewModel.MFACode);

                if (mfaResponse!=null && mfaResponse.IdToken.Length > 0)
                {
                    HttpContext?.Session.Set("IdToken", mfaResponse.IdToken);
                    HttpContext?.Session.Set("AccessToken", mfaResponse.AccessToken);
                    return RedirectToAction("LandingPage", "DigitalIdentity");
                }
                else
                {
                    ModelState.AddModelError("MFACode", "Enter a valid MFA code");
                    return View("MFAConfirmation");
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
            string accesstoken = HttpContext?.Session.Get<string>("AccessToken");
            HttpContext?.Session.Clear();
            _signUpService.SignOut(accesstoken);
            return RedirectToAction("LoginPage", "Login");
        }

    }
}