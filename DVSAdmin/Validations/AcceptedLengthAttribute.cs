﻿using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Validations
{
    /// <summary>
    /// Class to validate maximum length of the field
    /// The in built MaxLength field restricts entering the maxlimit
    /// but will not show error message. Hence adding as custom class
    /// </summary>
    /// <seealso cref="System.ComponentModel.DataAnnotations.ValidationAttribute" />
    [AttributeUsage(AttributeTargets.Property)]
    public class MaximumLengthAttribute : ValidationAttribute
    {
        public int acceptedLength { get; }

        public MaximumLengthAttribute(int acceptedLength)
        {
            this.acceptedLength = acceptedLength;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            string strValue = value.ToString() ?? string.Empty;
            if (strValue.Length > acceptedLength)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
