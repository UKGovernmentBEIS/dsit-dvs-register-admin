using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
    public class LoginViewModel :EnterEmailViewModel
    {
        [Required(ErrorMessage = "Enter a password")]       
        public string Password { get; set; }
    }
}
