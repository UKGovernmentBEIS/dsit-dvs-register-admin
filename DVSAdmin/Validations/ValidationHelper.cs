namespace DVSAdmin.Validations;

using DVSAdmin.CommonUtility;
using DVSAdmin.Models.Edit;
using Microsoft.AspNetCore.Mvc.ModelBinding;


public class ValidationHelper
{
    public record FieldComparisonConfig(
        string FirstFieldName,
        string SecondFieldName,
        string ErrorMessage
        );
    
    public static void ValidateDuplicateFields(ModelStateDictionary modelState, string? primaryValue, string? secondaryValue, FieldComparisonConfig config)
    {
        if (!string.IsNullOrEmpty(primaryValue) &&  !string.IsNullOrEmpty(secondaryValue) &&  primaryValue.Equals(secondaryValue, StringComparison.OrdinalIgnoreCase))
        { 
            modelState.AddModelError(config.SecondFieldName, config.ErrorMessage);
        }
    }


    public static DateTime? ValidateCustomExpiryDate(DateViewModel dateViewModel, DateTime issueDate, ModelStateDictionary modelstate)
    {
        DateTime? date = null;

        try
        {
            if (dateViewModel.Day == null || dateViewModel.Month == null || dateViewModel.Year == null)
            {
                if (dateViewModel.Day == null)
                {
                    modelstate.AddModelError("Day", Constants.ConformityExpiryDayError);
                }
                if (dateViewModel.Month == null)
                {
                    modelstate.AddModelError("Month", Constants.ConformityExpiryMonthError);
                }
                if (dateViewModel.Year == null)
                {
                    modelstate.AddModelError("Year", Constants.ConformityExpiryYearError);
                }
            }
            else
            {
                date = new DateTime(Convert.ToInt32(dateViewModel.Year), Convert.ToInt32(dateViewModel.Month), Convert.ToInt32(dateViewModel.Day));

                if (date <= DateTime.Today)
                {
                    modelstate.AddModelError("ValidDate", Constants.ConformityExpiryPastDateError);
                }

            }

        }
        catch (Exception)
        {
            modelstate.AddModelError("ValidDate", Constants.ConformityExpiryDateInvalidError);

        }
        return date;
    }



}