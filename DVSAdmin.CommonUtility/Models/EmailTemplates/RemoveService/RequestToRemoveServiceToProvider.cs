namespace DVSAdmin.CommonUtility.Models
{   
    
    public class RequestToRemoveServiceToProvider
    {    public string Id { get; set; }
    public string RecipientName { get; set; }
    public string ServiceName { get; set; }
    public string ReasonForRemoval { get; set; }
    public string RemovalLink { get; set; }
    }
}
