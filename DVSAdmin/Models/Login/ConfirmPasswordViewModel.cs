using System;
using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
	public class ConfirmPasswordViewModel
	{
       

        [Required(ErrorMessage = "Enter a valid password")]
        [StringLength(255, ErrorMessage = "Password must be between 10 and 255 characters", MinimumLength = 10)]
        [DataType(DataType.Password)]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[\\W_]).{10,}$", ErrorMessage = "Password must be at least 10 characters long and include a mix of letters, numbers, and symbols.")]
        public string? Password { get; set; }


        [Required(ErrorMessage = "Enter a valid password")]
        [StringLength(255, ErrorMessage = "Confirm Password must be between 10 and 255 characters", MinimumLength = 10)]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }


        [Required(ErrorMessage = "Enter a valid OTP")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The OTP must be a 6-digit number")]
        public string? OneTimePassword { get; set; }

        public bool? PasswordReset { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

