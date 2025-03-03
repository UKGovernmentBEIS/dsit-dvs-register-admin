namespace DVSAdmin.CommonUtility.Models
{
    public  class EditProviderRequestTemplate
    {
        public string Id { get; set; }
        public string RecipientName { get; set; }
        public string CompanyName { get; set; }
        public string PreviousData { get; set; }
        public string CurrentData { get; set; }
        public string ApproveLink { get; set; }
    }
}
