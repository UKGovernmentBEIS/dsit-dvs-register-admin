using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DVSAdmin.Validations
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AcceptedCharactersAttribute : ValidationAttribute
    {
        public string AcceptedCharactersPattern { get; }

        public AcceptedCharactersAttribute(string acceptedCharactersPattern)
        {
            AcceptedCharactersPattern = acceptedCharactersPattern;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            string strValue = value.ToString() ?? string.Empty;
            if (!Regex.IsMatch(strValue, AcceptedCharactersPattern))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
