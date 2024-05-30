﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Data.Entities
{
    public class SupplementaryScheme
    {
        public SupplementaryScheme() { }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string SchemeName { get; set; }
    }
}
