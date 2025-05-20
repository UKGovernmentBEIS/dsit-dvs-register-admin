namespace DVSAdmin.Models
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public string Action { get; set; }
        public string Controller { get; set; }
    }
}
