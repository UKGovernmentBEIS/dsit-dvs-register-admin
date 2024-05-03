using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Automapper
{
    public class URNStatusResolver : IValueResolver<UniqueReferenceNumber, UniqueReferenceNumberDto, URNStatusEnum>
    {
        public URNStatusEnum Resolve(UniqueReferenceNumber source, UniqueReferenceNumberDto destination, URNStatusEnum destMember, ResolutionContext context)
        {
            if (source.URNStatus == URNStatusEnum.Approved && source.ModifiedDate.HasValue)
            {
                TimeSpan difference = DateTime.Now - source.ModifiedDate.Value;
                if (difference.TotalDays > Constants.URNExpiryDays)
                {
                    return URNStatusEnum.Expired;
                }
            }

            return source.URNStatus;
        }
    }
    
}
