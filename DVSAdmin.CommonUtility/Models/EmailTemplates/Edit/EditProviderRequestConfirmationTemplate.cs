namespace DVSAdmin.CommonUtility.Models.EmailTemplates.Edit
{
     public class EditProviderRequestConfirmationTemplate
    {
        public string Id { get; set; }
        public string RecipientName { get; set; }
        public string CompanyName { get; set; }
        public string PreviousData { get; set; }
        public string CurrentData { get; set; }
    }
}
