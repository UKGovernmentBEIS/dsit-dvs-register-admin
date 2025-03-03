namespace DVSAdmin.CommonUtility.Models.EmailTemplates.Edit
{
    public class EditServiceRequestConfirmationTemplate
    {
        public string Id { get; set; }
        public string RecipientName { get; set; }
        public string CompanyName { get; set; }
        public string ServiceName { get; set; }
        public string PreviousData { get; set; }
        public string CurrentData { get; set; }
    }
}
