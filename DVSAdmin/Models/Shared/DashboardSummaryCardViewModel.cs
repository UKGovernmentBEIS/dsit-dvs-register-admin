namespace DVSAdmin.Models.Shared;

public class DashboardSummaryCardViewModel
{
    public string Label { get; set; } = "";
    public int Count { get; set; }
    public string Href { get; set; } = "";
    public bool Emphasised { get; set; }
}