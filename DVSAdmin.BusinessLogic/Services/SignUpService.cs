using System;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Email;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Repositories;

namespace DVSAdmin.BusinessLogic.Services
{
	public class SignUpService : ISignUpService
	{
        private CognitoClient _cognitoClient;
        private readonly IUserRepository _userRepository;
        private readonly IEmailSender _emailSender;

        public SignUpService(CognitoClient cognitoClient, IUserRepository userRepository, IEmailSender emailSender)
        {
            _userRepository = userRepository;
            _cognitoClient = cognitoClient;
            _emailSender = emailSender;
        }

        public async Task<GenericResponse> ConfirmPassword(string email, string password, string oneTimePassword)
        {
            return await _cognitoClient.ConfirmPasswordAndGenerateMFAToken(email, password, oneTimePassword);
        }

        public async Task<string> ForgotPassword(string email)
        {
            return await _cognitoClient.ForgotPassword(email);
        }

        public async Task<string> MFAConfirmation(string email, string password, string mfaCode)
        {
            
            if (await _cognitoClient.MFARegistrationConfirmation(email, password, mfaCode) == "OK")
            {
                GenericResponse genericResponse = await _userRepository.AddUser(new Data.Entities.User() { Email = email, UserName = email });
                if(genericResponse.Success)
                {
                    await _emailSender.SendAccountCreatedConfirmation(email, email);
                    return "OK";
                }
                else
                {
                    return "KO";
                }
            }
            else
            {
                return "KO";
            }
        }

        public async Task<string> SignInAndWaitForMfa(string email, string password)
        {
            string response = await _cognitoClient.SignInAndWaitForMfa(email, password);
            if (response == Constants.IncorrectPassword)
            {
                await _emailSender.SendFailedLoginAttempt(Helper.GetLocalDateTime(DateTime.UtcNow, "dd MMM yyyy h:mm tt"), email);
            }
            return response;
        }

        public async Task<string> ConfirmMFAToken(string session, string email, string token)
        {
            return await _cognitoClient.ConfirmMFAToken(session, email, token);
        }
    }
}

