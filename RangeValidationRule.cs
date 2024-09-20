using System.Globalization;
using System.Windows.Controls;

namespace CourseManagement
{
    public class RangeValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (string.IsNullOrWhiteSpace((value ?? "").ToString()))
            {
                return new ValidationResult(false, "Value cannot be empty.");
            }

            if (int.TryParse(value.ToString(), out int intValue))
            {
                if (intValue >= 0 && intValue <= 100)
                {
                    return ValidationResult.ValidResult;
                }
                return new ValidationResult(false, "Value must be between 0 and 100.");
            }

            return new ValidationResult(false, "Invalid number.");
        }
    }
}
