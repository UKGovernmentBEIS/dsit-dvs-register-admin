namespace DVSAdmin.CommonUtility.Models
{
    public class RequestToRemoveServiceNotificationToDSIT
    {
        public string Id { get; set; }
        public string CompanyName { get; set; }
        public string ServiceName { get; set; }
        public string ReasonForRemoval { get; set; }       
    }
}
