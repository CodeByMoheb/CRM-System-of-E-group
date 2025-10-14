using System.ComponentModel.DataAnnotations;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class OptionalEmailAttribute : ValidationAttribute
    {
        public OptionalEmailAttribute()
        {
            ErrorMessage = "Please enter a valid email address.";
        }

        public override bool IsValid(object? value)
        {
            // If the value is null or empty, it's valid (optional field)
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return true;
            }

            // If it has a value, validate it as an email
            var emailAttribute = new EmailAddressAttribute();
            return emailAttribute.IsValid(value);
        }
    }
}