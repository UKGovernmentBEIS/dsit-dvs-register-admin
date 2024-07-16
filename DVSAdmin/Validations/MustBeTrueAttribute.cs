using System.ComponentModel.DataAnnotations;

namespace DVSAdmin.Validations
{
    public class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return value is bool && (bool)value;
        }
    }
}
