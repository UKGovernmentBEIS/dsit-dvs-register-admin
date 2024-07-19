﻿using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
    public class MFARegistrationViewModel
	{
		public string? SecretToken { get; set; }
		public string? Email { get; set; }
		public string? Password { get; set; }

        [Required(ErrorMessage = "Enter a valid MFA code")]
        [RegularExpression("^[0-9]{6}$", ErrorMessage = "The MFA code must be a 6-digit number.")]
        public string MFACode { get; set; }
	}
}

