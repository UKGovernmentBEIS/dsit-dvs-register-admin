using DVSAdmin.BusinessLogic.Models;
using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models
{
    public class TFVersionViewModel 
    {
        public List<TrustFrameworkVersionDto>? AvailableVersions{ get; set; }

        [Required(ErrorMessage = "Select the TF Version")] //TO do : check error message
        public int? SelectedTFVersionId { get; set; }
        public TrustFrameworkVersionDto?  SelectedTFVersion { get; set; }

    }
}
