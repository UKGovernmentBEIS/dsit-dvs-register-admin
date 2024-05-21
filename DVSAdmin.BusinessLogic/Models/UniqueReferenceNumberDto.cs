using DVSAdmin.CommonUtility.Models.Enums;

namespace DVSAdmin.BusinessLogic.Models
{
    public class UniqueReferenceNumberDto
    {

        public int Id { get; set; }


        public required string URN { get; set; }

        public string? RegisteredDIPName { get; set; }

        public DateTime? LastCheckedTimeStamp { get; set; }

        public DateTime? ReleasedTimeStamp { get; set; }

        public string? CheckedByCAB { get; set; }

        public int? Validity { get; set; }

        public URNStatusEnum URNStatus { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
