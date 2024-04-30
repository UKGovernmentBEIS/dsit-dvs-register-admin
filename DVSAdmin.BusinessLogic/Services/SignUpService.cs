using System;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.Data.Repositories;

namespace DVSAdmin.BusinessLogic.Services
{
	public class SignUpService : ISignUpService
	{
        private CognitoClient _cognitoClient;
        private readonly IUserRepository _userRepository;

        public SignUpService(CognitoClient cognitoClient, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _cognitoClient = cognitoClient;
        }

        public async Task<string> ConfirmPassword(string email, string password, string oneTimePassword)
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
            return await _cognitoClient.SignInAndWaitForMfa(email, password);
        }

        public async Task<string> ConfirmMFAToken(string session, string email, string token)
        {
            return await _cognitoClient.ConfirmMFAToken(session, email, token);
        }
    }
}

