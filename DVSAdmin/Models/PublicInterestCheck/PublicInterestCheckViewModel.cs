using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models
{
    public class PublicInterestCheckViewModel
    {
        public List<PublicInterestCheckDto>? PrimaryChecksList { get; set; }

        public List<PublicInterestCheckDto>? SecondaryChecksList { get; set; }

        public List<PublicInterestCheckDto>? ArchiveList { get; set; }
    }
}
