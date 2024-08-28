using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Models
{
    public class ServiceQualityLevelMappingDto
    {
        public int Id { get; set; }      
        public int ServiceId { get; set; }
        public Service Service { get; set; }     
        public int QualityLevelId { get; set; }
        public QualityLevel QualityLevel { get; set; }
    }
}
