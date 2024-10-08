﻿using DVSAdmin.Data.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DVSAdmin.Data
{
    public class DVSAdminDbContext : DbContext, IDataProtectionKeyContext
    {
        public DVSAdminDbContext(DbContextOptions<DVSAdminDbContext> options) : base(options)
        {
        }
    
        public DbSet<User> User { get; set; }       
        public DbSet<CertificateReviewRejectionReason> CertificateReviewRejectionReason { get; set; }
        public DbSet<Role> Role { get; set; }
        public DbSet<IdentityProfile> IdentityProfile { get; set; }
        public DbSet<SupplementaryScheme> SupplementaryScheme { get; set; }      
        public DbSet<RegisterPublishLog> RegisterPublishLog { get; set; }
        public DbSet<PublicInterestCheck> PublicInterestCheck { get; set; }
        public DbSet<Cab> Cab { get; set; }
        public DbSet<CabUser> CabUser { get; set; }
        public DbSet<ProviderProfile> ProviderProfile { get; set; }
        public DbSet<Service> Service { get; set; }
        public DbSet<QualityLevel> QualityLevel { get; set; }
        public DbSet<ServiceQualityLevelMapping> ServiceQualityLevelMapping { get; set; }
        public DbSet<ServiceIdentityProfileMapping> ServiceIdentityProfileMapping { get; set; }
        public DbSet<ServiceRoleMapping> ServiceRoleMapping { get; set; }
        public DbSet<ServiceSupSchemeMapping> ServiceSupSchemeMapping { get; set; }
        public DbSet<CertificateReview> CertificateReview { get; set; }
        public DbSet<CertificateReviewRejectionReasonMapping> CertificateReviewRejectionReasonMapping { get; set; }
        public DbSet<ProceedApplicationConsentToken> ProceedApplicationConsentToken { get; set; }

        public DbSet<PICheckLogs> PICheckLogs { get; set; }
        public DbSet<ProceedPublishConsentToken> ProceedPublishConsentToken { get;set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<TrustmarkNumber> TrustmarkNumber { get; set; }


    }
}