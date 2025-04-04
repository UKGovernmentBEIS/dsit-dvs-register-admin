using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic
{
    public class DaysLeftResolverCertificateReview : IValueResolver<Service, ServiceDto, int>
    {
        public int Resolve(Service source, ServiceDto destination, int daysLeftToComplete, ResolutionContext context)
        {
            var date = source.ModifiedTime ?? source.CreatedTime;
            if (!date.HasValue)
                return 0;

            var daysPassed = (DateTime.UtcNow.Date - date.Value.Date).Days;
            var daysLeft = Constants.DaysLeftToCompleteCertificateReview - daysPassed;
            return Math.Max(0, daysLeft);
        }
    }
}
