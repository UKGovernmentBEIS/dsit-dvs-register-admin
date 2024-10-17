using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Models
{
    public class ProceedPublishConsentTokenDto
    {
        public string Id { get; set; }
        public string TokenId { get; set; }
        public string Token { get; set; }     
        public int ServiceId { get; set; }
        public Service Service { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
