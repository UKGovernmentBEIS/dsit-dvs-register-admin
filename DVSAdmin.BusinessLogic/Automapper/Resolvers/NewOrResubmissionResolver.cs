using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Automapper.Resolvers
{
    public class NewOrResubmissionResolver : IValueResolver<Service, ServiceDto, string>
    {
        public string Resolve(Service source, ServiceDto destination,string newOrResubmission, ResolutionContext context)
        {
            string result = string.Empty;
            if (source.ServiceVersion == 1)
                result = Constants.NewSubmission;
            else if(destination.ServiceVersion >1)
                result = Constants.ReSubmission;

            return result;
        }
    }
    
}
