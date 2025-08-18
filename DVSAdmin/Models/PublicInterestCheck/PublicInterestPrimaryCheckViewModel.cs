using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DVSAdmin.Models
{
    public class PublicInterestPrimaryCheckViewModel
    {        
       
        public int ServiceId { get; set; }
        public int ProviderProfileId { get; set; }       
        public ServiceDto? Service { get; set; }      

        [Required(ErrorMessage = "Select whether the public interest checks have been met")]
        public bool? PublicInterestChecksMet { get; set; }
        public ReviewTypeEnum ReviewType { get; set; }
        public PublicInterestCheckEnum PublicInterestCheckStatus { get; set; }
        [Required(ErrorMessage = "Enter the reason the check has failed")]
        public string? PrimaryCheckComment { get; set; }        
        public int? PrimaryCheckUserId { get; set; }
        public int? SecondaryCheckUserId { get; set; }

        [JsonIgnore]
        public PublicInterestCheckDto? PublicInterestCheck { get; set; }
    }
}
