using DVSAdmin.Data.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace DVSAdmin.BusinessLogic.Models.PreRegistration
{
    public class PreRegistrationCountryMappingDto
    {      
        public int PreRegistrationId { get; set; }
        public PreRegistrationDto PreRegistration { get; set; }

        [ForeignKey("Country")]
        public int CountryId { get; set; }
        public Country Country { get; set; }
    }
}
