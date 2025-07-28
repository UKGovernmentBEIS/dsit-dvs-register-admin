using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
    public class SchemeIdentityProfileMappingViewModel : ServiceSummaryBaseViewModel
    {      
        public int SchemeId { get; set; }
        [Required(ErrorMessage = "Select ‘Yes’ if the service is certified against GPG45")]
     
        public IdentityProfileViewModel IdentityProfile { get; set; }

    }
}
