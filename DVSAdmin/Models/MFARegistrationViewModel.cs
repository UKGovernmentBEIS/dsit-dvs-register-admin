using System;
namespace DVSAdmin.Models
{
	public class MFARegistrationViewModel
	{
		public string SecretToken { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
		public string MFACode { get; set; }
	}
}

