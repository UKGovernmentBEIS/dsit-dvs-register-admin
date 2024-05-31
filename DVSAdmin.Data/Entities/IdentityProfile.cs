using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Data.Entities
{
    public class IdentityProfile
    {
        public IdentityProfile() { }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string IdentityProfileName { get; set; }
    }
}
