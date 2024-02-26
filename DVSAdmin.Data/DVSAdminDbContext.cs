using Microsoft.EntityFrameworkCore;

namespace DVSAdmin.Data
{
    public class DVSAdminDbContext : DbContext
    {
        public DVSAdminDbContext(DbContextOptions<DVSAdminDbContext> options) : base(options)
        {
        }   

    }
}
