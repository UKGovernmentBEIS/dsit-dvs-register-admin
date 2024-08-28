﻿using System.ComponentModel.DataAnnotations;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using System.ComponentModel.DataAnnotations.Schema;

    namespace DVSAdmin.Data.Entities
{
    public class Service : BaseEntity
    {
        public Service() { }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("ProviderProfile")]
        public int ProviderProfileId { get; set; }
        public ProviderProfile Provider { get; set; }
        public string ServiceName { get; set; }
        public string WebsiteAddress { get; set; }
        public string CompanyAddress { get; set; }
        public ICollection<ServiceRoleMapping> ServiceRoleMapping { get; set; }
        public bool HasGPG44 { get; set; }       
        public ICollection<ServiceQualityLevelMapping>? ServiceQualityLevelMapping { get; set; }
        public bool HasGPG45 { get; set; }
        public ICollection<ServiceIdentityProfileMapping>? ServiceIdentityProfileMapping { get; set; }
        public bool HasSupplementarySchemes { get; set; }
        public ICollection<ServiceSupSchemeMapping>? ServiceSupSchemeMapping { get; set; }

        public string FileName { get; set; }
        public string FileLink { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal FileSizeInKb { get; set; }
        public DateTime ConformityIssueDate { get; set; }
        public DateTime ConformityExpiryDate { get; set; }
        
        //[ForeignKey("CabUser")]
        //public int CabUserId { get; set; }
        //public CabUser CabUser { get; set; }
        public int ServiceNumber { get;set; }
        public int TrustMarkNumber { get; set; }
        //public ServiceStatusEnum ServiceStatus { get; set; }
        //public NpgsqlTsVector SearchVector { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? ModifiedTime { get; set; }
        public DateTime? PublishedTime { get; set; }
    }
}
