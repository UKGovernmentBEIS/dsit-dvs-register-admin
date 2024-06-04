using DVSAdmin.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DVSAdmin.Data
{
    public class DVSAdminDbContext : DbContext
    {
        public DVSAdminDbContext(DbContextOptions<DVSAdminDbContext> options) : base(options)
        {
        }
        public DbSet<PreRegistration> PreRegistration { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<PreRegistrationCountryMapping> PreRegistrationCountryMapping { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<PreRegistrationReview> PreRegistrationReview { get; set; }
        public DbSet<UniqueReferenceNumber> UniqueReferenceNumber { get; set; }

        public DbSet<CertificateReview> CertificateReview { get; set; }
        public DbSet<CertificateReviewRejectionReason> CertificateReviewRejectionReason { get; set; }
        public DbSet<CertificateReviewRejectionReasonMappings> CertificateReviewRejectionReasonMappings { get; set; }

        public DbSet<CertificateInformation> CertificateInformation { get; set; }

        public DbSet<Role> Role { get; set; }
        public DbSet<CertificateInfoRoleMapping> CertificateInfoRoleMapping { get; set; }
        public DbSet<IdentityProfile> IdentityProfile { get; set; }
        public DbSet<CertificateInfoIdentityProfileMapping> CertificateInfoIdentityProfileMapping { get; set; }
        public DbSet<SupplementaryScheme> SupplementaryScheme { get; set; }
        public DbSet<CertificateInfoSupSchemeMapping> CertificateInfoSupSchemeMappings { get; set; }
    }
}
