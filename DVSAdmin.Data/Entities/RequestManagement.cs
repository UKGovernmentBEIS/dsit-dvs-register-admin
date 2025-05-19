using DVSAdmin.CommonUtility.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DVSAdmin.Data.Entities
{
    public class RequestManagement
    {
        public RequestManagement() { }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [ForeignKey("User")]
        public int InitiatedUserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Cab")]
        public int CabId { get; set; }
        public Cab Cab { get; set; }
        public RequestTypeEnum RequestType { get; set; }
        public RequestStatusEnum RequestStatus { get; set; }
        public DateTime ModifiedTime { get; set; }
    }
}
