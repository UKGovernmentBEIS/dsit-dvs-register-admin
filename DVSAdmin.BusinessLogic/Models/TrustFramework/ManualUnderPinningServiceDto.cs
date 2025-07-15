namespace DVSAdmin.BusinessLogic.Models
{
    public class ManualUnderPinningServiceDto
    {
       
        public int Id { get; set; }
        public string? ServiceName { get; set; }
        public string? ProviderName { get; set; }

        public string? SelectedCabName { get; set; }
        public int? CabId { get; set; }
        public CabDto Cab { get; set; }
        public DateTime? CertificateExpiryDate { get; set; }


    }
}
