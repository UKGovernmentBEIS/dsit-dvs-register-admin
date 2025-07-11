namespace DVSAdmin.Models
{
    public class ServiceSummaryBaseViewModel
    {      
        public bool FromSummaryPage { get; set; }
        public bool FromDetailsPage { get; set; } 
        public int SchemeId { get; set; }
        public string SchemeName { get; set; } = string.Empty;       
        public bool IsTfVersion0_4 { get; set; }  
        public int ServiceKey { get; set; }        
    }
}