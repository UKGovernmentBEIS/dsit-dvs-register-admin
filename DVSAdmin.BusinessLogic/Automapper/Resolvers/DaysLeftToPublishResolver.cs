using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic
{
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
