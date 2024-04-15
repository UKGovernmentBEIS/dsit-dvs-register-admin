using AutoMapper;
using DVSAdmin.Data.Entities;
using DVSAdmin.BusinessLogic.Models.PreRegistration;

namespace DVSAdmin.BusinessLogic.Automapper
{
    public class CommentsCountResolver : IValueResolver<PreRegistration, PreRegistrationDto, int>
    {
        public int Resolve(PreRegistration source, PreRegistrationDto destination, int destMember, ResolutionContext context)
        {
            //TODO: implement logic to get comments count
            return 0;
        }
    }


}
