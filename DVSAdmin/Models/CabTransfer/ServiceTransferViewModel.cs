using DVSAdmin.BusinessLogic.Models;
using DVSAdmin.CommonUtility.Models.Enums;

namespace DVSAdmin.Models.CabTransfer
{
    public class ServiceTransferViewModel
    {
        public ServiceDto Service { get; set; }
        public bool IsServiceDetailsPage { get; set; }
        public string? ToCabName { get; set; }
    }
}
