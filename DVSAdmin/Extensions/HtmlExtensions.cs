﻿using DVSAdmin.CommonUtility.Models;
using DVSAdmin.CommonUtility.Models.Enums;
using Microsoft.AspNetCore.Html;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Web;

namespace DVSAdmin.Extensions
{
    public static class HtmlExtensions
    {

        private static string GetTagClass<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            
            switch (value)
            {
                case ApplicationReviewStatusEnum.Received: 
                case ProviderStatusEnum.Published:
                case ServiceStatusEnum.Submitted:
                case ServiceStatusEnum.Published:
                case ServiceStatusEnum.Received:
                return "govuk-tag govuk-tag--blue";


                case ApplicationReviewStatusEnum.PrimaryCheckPassed:                        
                case ApplicationReviewStatusEnum.ApplicationApproved:
                case CertificateReviewEnum.Approved:
                case PublicInterestCheckEnum.PrimaryCheckPassed:
                case PublicInterestCheckEnum.PublicInterestCheckPassed:
                return "govuk-tag govuk-tag--green";

                case ApplicationReviewStatusEnum.SentBackBySecondReviewer:                   
                case ApplicationReviewStatusEnum.PrimaryCheckFailed:                   
                case ApplicationReviewStatusEnum.ApplicationRejected: 
                              
             
                case ProviderStatusEnum.RemovedFromRegister:
                case CertificateReviewEnum.Rejected:
                case PublicInterestCheckEnum.PublicInterestCheckFailed:
                case PublicInterestCheckEnum.PrimaryCheckFailed:
                case PublicInterestCheckEnum.SentBackBySecondReviewer:
                return "govuk-tag govuk-tag--red";



                case ApplicationReviewStatusEnum.InPrimaryReview: 
                case ProviderStatusEnum.ActionRequired:
                case ProviderStatusEnum.PublishedActionRequired:
                case ServiceStatusEnum.ReadyToPublish:
                case CertificateReviewEnum.InReview:
                case PublicInterestCheckEnum.InPrimaryReview:
                case PublicInterestCheckEnum.PrimaryCheckSkipped:
                    return "govuk-tag govuk-tag--yellow";

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


        public static HtmlString ToStringWithLineBreaks(string input)
        {
           
            string output = input?.Replace("\r", "<br>")??string.Empty;       

            return new HtmlString(output);
        }


        //For displaying statuses of each section in secondary review details, whcih are approved /rejected in primary check

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
