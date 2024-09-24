using DVSAdmin.BusinessLogic.Models.CertificateReview;
using DVSAdmin.Data.Entities;
using System.Text.Json.Serialization;

namespace DVSAdmin.BusinessLogic.Models
{
    public class ServiceQualityLevelMappingDto
    {
        public int Id { get; set; }      
        public int ServiceId { get; set; }

        [JsonIgnore]
        public ServiceDto Service { get; set; }     
        public int QualityLevelId { get; set; }
        public QualityLevelDto QualityLevel { get; set; }
    }
}
