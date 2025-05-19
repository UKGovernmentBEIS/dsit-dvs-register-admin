using DVSAdmin.BusinessLogic.Models;

namespace DVSAdmin.Models.CabTransfer
{
    public class SelectCabViewModel
    { 
        public IReadOnlyList<CabDto> Cabs { get; set; }
        public int? SelectedCabId { get; set; }
        public int CurrentCabId { get; set; }
        public int ServiceId { get; set; }

    }
}