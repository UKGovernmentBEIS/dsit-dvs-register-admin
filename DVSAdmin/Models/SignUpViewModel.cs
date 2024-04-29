using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
	public class SignUpViewModel
	{
        [Required(ErrorMessage = "Enter an email address in the correct format, like name@example.com.")]
        [EmailAddress(ErrorMessage = "Enter an email address in the correct format, like name@example.com.")]
        public string Email { get; set; }

		public string Password { get; set;  }
		public string ConfirmPassword { get; set; }
		public string OneTimePassword { get; set; }
		public string MFAConfirmationCode { get; set; }
	}
}

