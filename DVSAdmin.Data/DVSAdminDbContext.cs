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
    }
}
