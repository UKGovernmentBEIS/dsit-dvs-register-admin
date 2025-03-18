namespace DVSAdmin.Models
{
    public class ReviewFiledsViewModel
    {
        public CertificateValidationViewModel? CertificateValidation { get; set; }
        public bool? InformationMatched { get; set; }

        public bool IsRejectFlow { get; set; }

        public string? TextForIncorrectField { get; set; }
    }
}
