namespace DVSAdmin.Models.Edit
{
    public class DateViewModel : ServiceSummaryBaseViewModel
    {
        public int? Day { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public string? ValidDate { get; set; }
        public bool FromSummaryPage { get; set; }
        public string? PropertyName { get; set; }

    }
}

