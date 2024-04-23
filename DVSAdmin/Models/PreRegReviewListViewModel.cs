using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.PreRegistration;

namespace DVSAdmin.Models
{
    public class PreRegReviewListViewModel
    {
        public List<PreRegistrationDto>? PrimaryChecksList { get; set; }

        public List<PreRegistrationDto>? SecondaryChecksList { get; set; }

        public List<UniqueReferenceNumberDto>? ArchiveList { get; set; }
    }
}
