using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;

namespace DVSAdmin.Models
{
    public class ViewModelHelper
    {
        public static List<string> GetCabsForProvider(List<ServiceDto> services)
        {
            List<string> disctinctCabs = services.Select(s => s.CabUser.Cab.CabName).Distinct().ToList();

            if(disctinctCabs!=null && disctinctCabs.Count>1  )
            {
                if (!services.All(s => s.ServiceStatus == ServiceStatusEnum.Removed))
                {
                    disctinctCabs = services.Where(s => s.ServiceStatus == ServiceStatusEnum.ReadyToPublish ||
                  s.ServiceStatus == ServiceStatusEnum.Published ||
                  s.ServiceStatus == ServiceStatusEnum.PublishedUnderReassign ||
                  s.ServiceStatus == ServiceStatusEnum.RemovedUnderReassign ||
                  s.ServiceStatus == ServiceStatusEnum.CabAwaitingRemovalConfirmation ||
                  s.ServiceStatus == ServiceStatusEnum.UpdatesRequested)
                 .Select(s => s.CabUser.Cab.CabName).Distinct().ToList();
                }

                  
            }


            return disctinctCabs??[];
        }

        public static string GetServiceType(ServiceTypeEnum? input)
        {
            if (input == ServiceTypeEnum.UnderPinning) return "Underpinning";
            else if (input == ServiceTypeEnum.WhiteLabelled) return "White-labelled";
            else return "Neither";


        }
    }
}
