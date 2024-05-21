using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models
{
    public class PreRegReviewListViewModel
    {
        public List<PreRegistrationDto>? PrimaryChecksList { get; set; }

        public List<PreRegistrationDto>? SecondaryChecksList { get; set; }

        public List<PreRegistrationDto>? ArchiveList { get; set; }
    }
}
