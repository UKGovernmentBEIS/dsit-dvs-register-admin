using System;
namespace DVSAdmin.BusinessLogic.Services
{
	public interface ISignUpService
	{
		public Task<string> ForgotPassword(string email);

		public Task<string> ConfirmPassword(string email, string password, string oneTimePassword);

		public Task<string> MFAConfirmation(string email, string password, string mfaCode);

		public Task<string> SignInAndWaitForMfa(string email, string password);

		public Task<string> ConfirmMFAToken(string session, string email, string token);
    }
}

