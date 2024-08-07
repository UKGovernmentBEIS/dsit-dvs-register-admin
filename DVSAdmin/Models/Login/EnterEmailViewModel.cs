using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
    public class EnterEmailViewModel
    {
        [Required(ErrorMessage = "Enter an email address")]
        [EmailAddress(ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
        //[RegularExpression(@"^[a-zA-Z0-9._%+-]+@([a-zA-Z0-9.-]+\.)?gov\.uk$", ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
        public string? Email { get; set; }
        public bool? PasswordReset { get; set; }
    }
}
