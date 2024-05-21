using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
	public class SignUpViewModel
	{
        [Required(ErrorMessage = "Enter an email address in the correct format, like name@example.com.")]
        [EmailAddress(ErrorMessage = "Enter an email address in the correct format, like name@example.com.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@([a-zA-Z0-9.-]+\.)?gov\.uk$", ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Enter a valid password")]
        [StringLength(255, ErrorMessage = "Password must be between 10 and 255 characters", MinimumLength = 10)]
        [DataType(DataType.Password)]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[\\W_]).{10,}$", ErrorMessage = "Password must be at least 10 characters long and include a mix of letters, numbers, and symbols.")]
        public string? Password { get; set; }


        [Required(ErrorMessage ="Enter a valid password")]
        [StringLength(255, ErrorMessage = "Confirm Password must be between 10 and 255 characters", MinimumLength = 10)]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        public string? OneTimePassword { get; set; }

        [RegularExpression("^[0-9]{6}$", ErrorMessage = "The MFA confirmation code must be a 6-digit number.")]
        public string? MFAConfirmationCode { get; set; }
	}
}

