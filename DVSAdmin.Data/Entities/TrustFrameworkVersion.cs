using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DVSAdmin.Data.Entities
{
    public class TrustFrameworkVersion
    {       
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string TrustFrameworkName { get; set; }

        public Decimal Version { get; set; }
        public int Order { get; set; }
    }
}
