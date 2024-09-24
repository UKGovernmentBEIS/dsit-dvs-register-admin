using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Models
{
    public class CabUserDto
    {      
        public int Id { get; set; }     
        public int CabId { get; set; }
        public Cab Cab { get; set; }
        public string CabEmail { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
