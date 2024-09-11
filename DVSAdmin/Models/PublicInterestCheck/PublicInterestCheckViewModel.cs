using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models
{
    public class PublicInterestCheckViewModel
    {
        public List<ServiceDto>? PrimaryChecksList { get; set; }

        public List<ServiceDto>? SecondaryChecksList { get; set; }

        public List<ServiceDto>? ArchiveList { get; set; }
    }
}
