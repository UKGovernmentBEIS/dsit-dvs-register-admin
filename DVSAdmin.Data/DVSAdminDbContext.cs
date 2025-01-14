using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;
using DVSAdmin.Data.Models;
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
        public DbSet<Event> EventLogs { get; set; }
        public DbSet<RemovalReasons> RemovalReasons { get; set; }
        public DbSet<RemoveProviderToken> RemoveProviderToken { get; set; }
        public DbSet<RemoveTokenServiceMapping> RemoveTokenServiceMapping { get; set; }
        public virtual async Task<int> SaveChangesAsync(TeamEnum team = TeamEnum.NA, EventTypeEnum eventType = EventTypeEnum.NA, string actorId = null)
        {
            if (actorId !=null)
            {
                OnBeforeSaveChanges(team, eventType, actorId);
            }
            var result = await base.SaveChangesAsync();
            return result;
        }
        private void OnBeforeSaveChanges(TeamEnum team, EventTypeEnum eventType, string actorId)
        {
            ChangeTracker.DetectChanges();
            var eventEntries = new List<EventEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Event || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;
                var eventEnty = new EventEntry(entry);
                eventEnty.TableName = entry.Entity.GetType().Name;
                eventEnty.ActorId = actorId;
                eventEnty.Team = team;
                eventEnty.EventType = eventType;
                eventEntries.Add(eventEnty);
                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        eventEnty.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            eventEnty.Action = ActionEnum.Create;
                            eventEnty.NewValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            eventEnty.Action = ActionEnum.Delete;
                            eventEnty.OldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                eventEnty.ChangedColumns.Add(propertyName);
                                eventEnty.Action = ActionEnum.Update;
                                eventEnty.OldValues[propertyName] = property.OriginalValue;
                                eventEnty.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }
            foreach (var eventEntry in eventEntries)
            {
                EventLogs.Add(eventEntry.ToEventLogs());
            }
        }


    }
}