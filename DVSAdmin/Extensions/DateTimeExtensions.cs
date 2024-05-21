using Microsoft.AspNetCore.Html;


namespace DVSAdmin.Extensions
{
    public static class DateTimeExtensions
    {
        public static HtmlString FormatDateTime(DateTime? dateTime)
        {
            DateTime dateTimeValue = Convert.ToDateTime(dateTime);
            TimeZoneInfo localTimeZone = TimeZoneInfo.Local; // Get local time zone
            DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(dateTimeValue, localTimeZone); // Convert to local time
            string time = localTime.ToString("h:mm tt");
            string date = localTime.ToString("d MMM yyyy");
            return new HtmlString($"{time};<br/>{date}") ;
        }
    }
}
