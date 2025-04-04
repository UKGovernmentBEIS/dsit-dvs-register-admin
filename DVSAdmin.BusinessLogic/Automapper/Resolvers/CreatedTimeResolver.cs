using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic
{
    public class CreatedTimeResolver : IValueResolver<Service, ServiceDto, DateTime?>
    {
        public DateTime? Resolve(Service source, ServiceDto destination, DateTime? createdTime, ResolutionContext context)
        {
            if (source.ResubmissionTime.HasValue)
                return source.ResubmissionTime;
            else return source.CreatedTime;
        }
    }
}
