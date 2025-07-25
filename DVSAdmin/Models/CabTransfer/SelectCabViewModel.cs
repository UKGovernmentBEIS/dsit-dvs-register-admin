using DVSAdmin.BusinessLogic.Models;
using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Models.CabTransfer
{
    public class SelectCabViewModel : ServiceSummaryBaseViewModel
    { 
        public IReadOnlyList<CabDto> Cabs { get; set; }
        [Required(ErrorMessage = "Select the CAB this service should be reassigned to")]
        public int? SelectedCabId { get; set; }
        public int CurrentCabId { get; set; }
        public string? SelectedCabName { get; set; }
        public string CurrentCabName { get; set; }
        public int ServiceId { get; set; }

    }
}