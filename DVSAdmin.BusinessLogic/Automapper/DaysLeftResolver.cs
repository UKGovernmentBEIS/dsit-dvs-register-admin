using AutoMapper;
using DVSAdmin.Data.Entities;
using DVSAdmin.BusinessLogic.Models.PreRegistration;

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

                // Calculate the number of days left (14 days minus days passed)
                var daysLeft = 14 - daysPassed;

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
}
