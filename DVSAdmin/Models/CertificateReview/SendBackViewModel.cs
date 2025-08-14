using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models.CertificateReview
{
    public class SendBackViewModel :ReviewViewModel
    {
        [Required(ErrorMessage = "Enter the amendments needed for the CAB")]
        public string? Reason { get; set; }
       
    }
}


