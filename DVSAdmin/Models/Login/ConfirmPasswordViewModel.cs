using System;
using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
	public class ConfirmPasswordViewModel
	{
       

        [Required(ErrorMessage = "Enter a valid password")]
        [MinLength(8, ErrorMessage = "Password length must be minimum 8 characters")]
        [DataType(DataType.Password)]       
        public string? Password { get; set; }


        [Required(ErrorMessage = "Enter a valid password")]
        [MinLength(8, ErrorMessage = "Password length must be minimum 8 characters")]
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

