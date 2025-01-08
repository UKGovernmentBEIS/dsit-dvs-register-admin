namespace DVSAdmin.BusinessLogic.Models
{
    public class RemovalReasonDto
    {
        public int RemovalReasonId { get; set; }
        public string RemovalReason { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeUpdated { get; set; }
        public bool IsActiveReason { get; set; }
        public bool RequiresAdditionalInfo { get; set; }
    }
}


