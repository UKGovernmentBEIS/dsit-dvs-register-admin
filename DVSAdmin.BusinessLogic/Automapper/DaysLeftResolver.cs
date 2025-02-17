using AutoMapper;
using DVSAdmin.Data.Entities;
using DVSAdmin.CommonUtility;
using DVSAdmin.BusinessLogic.Models;


namespace DVSAdmin.BusinessLogic.Extensions
{
    

    public class DaysLeftResolverCertificateReview : IValueResolver<Service, ServiceDto, int>
    {
        public int Resolve(Service source, ServiceDto destination, int daysLeftToComplete, ResolutionContext context)
        {
            if (source.CreatedTime.HasValue)
            {               
                var daysPassed = (DateTime.UtcNow.Date - source.ModifiedTime.Value.Date).Days;               
                var daysLeft = Constants.DaysLeftToCompleteCertificateReview - daysPassed;
                return Math.Max(0, daysLeft);
            }
            else
            {
               
                return 0;
            }
        }
    }


    public class DaysLeftResolverPICheck : IValueResolver<Service, ServiceDto, int>
    {
        public int Resolve(Service source, ServiceDto destination, int daysLeftToCompletePICheck, ResolutionContext context)
        {
            if (source.ModifiedTime.HasValue)
            {
                var daysPassed = (DateTime.UtcNow.Date - source.ModifiedTime.Value.Date).Days;
                var daysLeft = Constants.DaysLeftToCompletePICheck - daysPassed;
                return Math.Max(0, daysLeft);
            }
            else
            {

                return 0;
            }
        }
    }

    public class DaysLeftToPublishResolver : IValueResolver<ProviderProfile, ProviderProfileDto, int>
    {
        public int Resolve(ProviderProfile source, ProviderProfileDto destination, int daysLeftToComplete, ResolutionContext context)
        {
            if (source.ModifiedTime.HasValue)
            {                
                var daysPassed = (DateTime.UtcNow - source.ModifiedTime.Value).Days;
                var daysLeft = Constants.DaysLeftToPublish - daysPassed;
                return Math.Max(0, daysLeft);
            }
            else
            {
                return 0;
            }
        }
    }
}
