namespace DVSAdmin.Models
{
    public class PaginationViewModel
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string Sort { get; set; }
        public string SortAction { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
    }
}
