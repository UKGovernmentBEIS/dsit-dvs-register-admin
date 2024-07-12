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
                SignUpViewModel signUpViewModel = new SignUpViewModel();
                signUpViewModel.PasswordReset = passwordReset;
                return View("EnterEmail", signUpViewModel);
            }
            return View("EnterEmail");
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
                    return RedirectToAction("ConfirmPassword", "Login", new { passwordReset = signUpViewModel.PasswordReset });
                }
                else
                {
                    ModelState.AddModelError("Email", "Incorrect Email provided");
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
            confirmPasswordViewModel.Email = HttpContext?.Session.Get<string>("Email");
            confirmPasswordViewModel.PasswordReset = passwordReset;
            return View("ConfirmPassword", confirmPasswordViewModel);
        }

        [HttpPost("confirm-password")]
        public async Task<IActionResult> ConfirmPasswordCheck(ConfirmPasswordViewModel confirmPasswordViewModel)
        {
            confirmPasswordViewModel.Email = HttpContext?.Session.Get<string>("Email");
            if (ModelState["Password"].Errors.Count ==0 && ModelState["ConfirmPassword"].Errors.Count==0)
            {
                GenericResponse confirmPasswordResponse = new GenericResponse();
                if(confirmPasswordViewModel.PasswordReset!= null && confirmPasswordViewModel.PasswordReset ==true)
                {
                    confirmPasswordResponse = await _signUpService.ResetPassword(confirmPasswordViewModel.Email, confirmPasswordViewModel.Password, confirmPasswordViewModel.OneTimePassword);
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
                    confirmPasswordResponse = await _signUpService.ConfirmPassword(confirmPasswordViewModel.Email, confirmPasswordViewModel.Password, confirmPasswordViewModel.OneTimePassword);
                    if (confirmPasswordResponse.Success)
                    {
                        HttpContext?.Session.Set("MFARegistrationViewModel", new MFARegistrationViewModel { Email = confirmPasswordViewModel.Email, Password = confirmPasswordViewModel.Password, SecretToken = confirmPasswordResponse.Data });
                        return RedirectToAction("MFARegistration", "Login");
                    }
                    else
                    {
                        ModelState.AddModelError("ErrorMessage", "Error in setting password");                      
                        return View("ConfirmPassword", confirmPasswordViewModel);
                    }
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
                    return View("SignUpComplete");
                }
                else
                {
                    ModelState.AddModelError("MFACode", "Wrong MFA Code Provided from Authenticator App");
                    return View("MFARegistration", viewModel);
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
                if (loginResponse.Length > 0 && loginResponse != Constants.IncorrectPassword)
                {
                    HttpContext?.Session.Set("Email", loginPageViewModel.Email);
                    HttpContext?.Session.Set("Session", loginResponse);
                    return RedirectToAction("MFAConfirmation");
                }
                else
                {
                    ModelState.AddModelError("Email", "Incorrect email or password");
                    ModelState.AddModelError("Password", "Incorrect email or password");
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
        public async Task<IActionResult> ConfirmMFACodeLogin(LoginPageViewModel loginPageViewModel)
        {
            if (ModelState["MFACode"].Errors.Count == 0)
            {
                var mfaResponse = await _signUpService.ConfirmMFAToken(HttpContext?.Session.Get<string>("Session"), HttpContext?.Session.Get<string>("Email"), loginPageViewModel.MFACode);

                if (mfaResponse.IdToken.Length > 0)
                {
                    HttpContext?.Session.Set("IdToken", mfaResponse.IdToken);
                    HttpContext?.Session.Set("AccessToken", mfaResponse.AccessToken);
                    return RedirectToAction("LandingPage", "OfDia");
                }
                else
                {
                    ModelState.AddModelError("MFACode", "There is a problem with your MFA Code");
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
