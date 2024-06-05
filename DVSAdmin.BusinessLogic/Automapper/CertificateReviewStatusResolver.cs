using AutoMapper;
using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility;
using DVSAdmin.CommonUtility.Models.Enums;
using DVSAdmin.Data.Entities;

namespace DVSAdmin.BusinessLogic.Automapper
{
    public class CertificateReviewStatusResolver : IValueResolver<CertificateInformation, CertificateInformationDto,  CertificateInfoStatusEnum>
    {
        public CertificateInfoStatusEnum Resolve(CertificateInformation source, CertificateInformationDto destination, CertificateInfoStatusEnum destMember, ResolutionContext context)
        {
            if (source.CertificateInfoStatus == CertificateInfoStatusEnum.Received && source.CreatedDate.HasValue)
            {
                TimeSpan difference = DateTime.Now - source.CreatedDate.Value;
                if (difference.TotalDays > Constants.DaysLeftToCompleteCertificateReview)
                {
                    return CertificateInfoStatusEnum.Expired;
                }
            }

            return source.CertificateInfoStatus;
        }
    }
    
}
