using AutoMapper;
using DVSAdmin.Data.Entities;
using DVSAdmin.CommonUtility;
using DVSAdmin.BusinessLogic.Models;


namespace DVSAdmin.BusinessLogic.Extensions
{
    public class DaysLeftResolver : IValueResolver<PreRegistration, PreRegistrationDto, int>
    {
        public int Resolve(PreRegistration source, PreRegistrationDto destination, int daysLeftToComplete, ResolutionContext context)
        {
            if (source.CreatedDate.HasValue)
            {
                // Calculate the difference in days from the created date to today
                var daysPassed = (DateTime.Today - source.CreatedDate.Value).Days;

                // Calculate the number of days left (60 days minus days passed)
                var daysLeft = Constants.DaysLeftToComplete - daysPassed;

                // Ensure days left is not negative
                return Math.Max(0, daysLeft);
            }
            else
            {
                // If CreatedDate is null, return 0 days left
                return 0;
            }
        }
    }

    public class DaysLeftResolverCertificateReview : IValueResolver<Service, ServiceDto, int>
    {
        public int Resolve(Service source, ServiceDto destination, int daysLeftToComplete, ResolutionContext context)
        {
            if (source.CreatedTime.HasValue)
            {               
                var daysPassed = (DateTime.Today - source.CreatedTime.Value).Days;               
                var daysLeft = Constants.DaysLeftToCompleteCertificateReview - daysPassed;
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
                var daysPassed = (DateTime.Today - source.ModifiedTime.Value).Days;
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
