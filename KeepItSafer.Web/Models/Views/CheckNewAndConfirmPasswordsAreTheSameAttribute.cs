using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace KeepItSafer.Web.Models.Views
{
    public class CheckNewAndConfirmPasswordsAreTheSameAttribute : ValidationAttribute
    {
        private readonly string _comparisonProperty;

        public CheckNewAndConfirmPasswordsAreTheSameAttribute(string comparisonProperty)
        {
            _comparisonProperty = comparisonProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            ErrorMessage = ErrorMessageString;
            var currentValue = value as string;

            var property = validationContext.ObjectType.GetProperty(_comparisonProperty);

            if (property == null)
            {
                throw new ArgumentException("Property with this name not found");
            }

            var comparisonValue = property.GetValue(validationContext.ObjectInstance) as string;

            if (!string.Equals(currentValue, comparisonValue, StringComparison.CurrentCulture))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}