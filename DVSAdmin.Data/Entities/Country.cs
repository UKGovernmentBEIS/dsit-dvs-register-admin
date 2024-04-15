using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Data.Entities
{
    public  class Country
    {
        public Country() { }

        [Key]
        public int Id { get; set; }
        public string CountryName { get; set; }
    }
}
