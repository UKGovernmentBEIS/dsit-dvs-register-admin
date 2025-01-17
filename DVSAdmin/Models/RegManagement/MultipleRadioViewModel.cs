using DVSAdmin.CommonUtility.Models.Enums;

namespace DVSAdmin.Models.RegManagement
{
    public class MultipleRadiosViewModel
    {
        public int ProviderId { get; set; }
        public RemovalReasonsEnum? SelectedRemovalReason { get; set; }
        public List<RemovalReasonViewModel>? RemovalReasons { get; set; }
    }

    public class RemovalReasonViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public bool RequiresAdditionalInfo { get; set; }
    }
}