using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.BusinessLogic.Models.PublicInterestCheck;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Validations;
namespace DVSAdmin.Models
{
    public class PublicInterestSecondaryCheckViewModel
    {        
       
        public int ServiceId { get; set; }
        public int ProviderProfileId { get; set; }      
        public ServiceDto? Service { get; set; }
        public bool? IsCompanyHouseNumberApproved { get; set; }
        public bool? IsDirectorshipsApproved { get; set; }
        public bool? IsDirectorshipsAndRelationApproved { get; set; }
        public bool? IsTradingAddressApproved { get; set; }
        public bool? IsSanctionListApproved { get; set; }
        public bool? IsUNFCApproved { get; set; }
        public bool? IsECCheckApproved { get; set; }
        public bool? IsTARICApproved { get; set; }
        public bool? IsBannedPoliticalApproved { get; set; }
        public bool? IsProvidersWebpageApproved { get; set; }
        public ReviewTypeEnum ReviewType { get; set; }
        public PublicInterestCheckEnum PublicInterestCheckStatus { get; set; }
        public string? PrimaryCheckComment { get; set; }
        public string? SecondaryCheckComment { get; set; }
        public string? SubmitValidation { get; set; }
        public int? PrimaryCheckUserId { get; set; }
        public int? SecondaryCheckUserId { get; set; }
        public List<RejectionReasonDto>? AvailableReasons { get; set; } = new List<RejectionReasonDto>
        {
        new RejectionReasonDto { Id = 1, RejectionReasonName = "Failed due diligence check" },
        new RejectionReasonDto { Id = 2, RejectionReasonName = "Submitted incorrect information" }      
        };

      
        public List<int>? SelectedReasonIds { get; set; }
        public List<RejectionReasonDto>? SelectedReasons { get; set; }
        public PublicInterestCheckDto? PublicInterestCheck { get; set; }
    }
}
