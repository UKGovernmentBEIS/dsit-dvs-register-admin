using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DVSAdmin.Data.Entities
{
    public class Cab
    {
        public Cab() { }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string CabName { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
