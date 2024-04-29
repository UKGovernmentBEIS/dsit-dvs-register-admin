using DVSAdmin.CommonUtility.Models.Enums;
using Microsoft.AspNetCore.Html;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Web;

namespace DVSAdmin.Extensions
{
    public static class EnumHtmlExtensions
    {

        private static string GetTagClass<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            switch (value)
            {
                case ApplicationReviewStatusEnum.Received:
                    return "govuk-tag govuk-tag--blue";
                case ApplicationReviewStatusEnum.InPrimaryReview:
                    return "govuk-tag govuk-tag--yellow";
                case ApplicationReviewStatusEnum.PrimaryCheckFailed:
                    return "govuk-tag govuk-tag--red";
                case ApplicationReviewStatusEnum.PrimaryCheckPassed:
                    return "govuk-tag govuk-tag--green";
                case ApplicationReviewStatusEnum.ApplicationRejected:
                    return "govuk-tag govuk-tag--red";
                case ApplicationReviewStatusEnum.ApplicationApproved:
                    return "govuk-tag govuk-tag--green";
                case URNStatusEnum.Expired:
                    return "govuk-tag govuk-tag--red";
                case ApplicationReviewStatusEnum.SentBackBySecondReviewer:
                    return "govuk-tag govuk-tag--red";
                case URNStatusEnum.Created:
                    return "govuk-tag govuk-tag--blue";
                case URNStatusEnum.Approved:
                    return "govuk-tag govuk-tag--green";
                case URNStatusEnum.Rejected:
                    return "govuk-tag govuk-tag--red";
                case URNStatusEnum.ValidatedByCAB:
                    return "govuk-tag govuk-tag--blue";
                default:
                    return string.Empty;
            }
        }

        public static HtmlString ToStyledStrongTag<TEnum>(this TEnum enumValue) where TEnum : struct, Enum
        {
            string tagClass = GetTagClass(enumValue);
            string description = GetDescription(enumValue);

            StringBuilder sb = new StringBuilder();
            sb.Append("<strong class=\"");
            sb.Append(HttpUtility.HtmlEncode(tagClass));
            sb.Append("\">");
            sb.Append(HttpUtility.HtmlEncode(description));
            sb.Append("</strong>");
            var test = sb.ToString();
            return new HtmlString(sb.ToString());
        }



        //For displaying statuses in secondary review, whcih are approved /rejected in primary check

        public static HtmlString ToStyledStrongTag(this bool? input)
        {
            bool boolValue = Convert.ToBoolean(input);
            string tagClass = boolValue ? "govuk-tag govuk-tag--green" : "govuk-tag govuk-tag--red";
            string description = boolValue ? "Approved" : "Rejected";

            StringBuilder sb = new StringBuilder();
            sb.Append("<strong class=\"");
            sb.Append(HttpUtility.HtmlEncode(tagClass));
            sb.Append("\">");
            sb.Append(HttpUtility.HtmlEncode(description));
            sb.Append("</strong>");

            return new HtmlString(sb.ToString());
        }

        private static string GetDescription<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            FieldInfo field = value.GetType().GetField(value.ToString());
            DescriptionAttribute attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute == null ? value.ToString() : attribute.Description;
        }

    }
}
