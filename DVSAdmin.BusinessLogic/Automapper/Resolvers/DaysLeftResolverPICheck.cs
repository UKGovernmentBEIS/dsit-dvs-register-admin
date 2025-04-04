using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic
{
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
}
