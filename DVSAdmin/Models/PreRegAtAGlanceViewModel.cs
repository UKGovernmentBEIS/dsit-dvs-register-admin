namespace DVSAdmin.Models
{
    public class PreRegAtAGlanceViewModel
    {
        public int LessThanAWeekApplicationCount { get; set; }
        public int InReviewApplicationCount { get; set; }
        public int URNExpiredCount { get; set; }
        public int URNNotValidatedCount { get; set; }
    }
}
