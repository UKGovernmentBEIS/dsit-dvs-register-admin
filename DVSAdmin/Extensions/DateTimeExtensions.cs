using Microsoft.AspNetCore.Html;


namespace DVSAdmin.Extensions
{
    public static class DateTimeExtensions
    {
        public static HtmlString FormatDateTime(DateTime? dateTime)
        {
            DateTime dateTimeValue = Convert.ToDateTime(dateTime);
            string time = dateTimeValue.ToLocalTime().ToString("h:mm tt");
            string date = dateTimeValue.ToLocalTime().ToString("d MMM yyyy");
            return new HtmlString($"{time};<br/>{date}") ;
        }
    }
}
