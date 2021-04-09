using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CVGS.Models.Validators
{
    public class CustomCreditCardAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // this validator is not a replacement for [Required]
            if (value == null)
            {
                return ValidationResult.Success;
            }
            string cardNumber = value.ToString();
            if (string.IsNullOrEmpty(cardNumber))
            {
                return ValidationResult.Success;
            }
            // remove all spaces
            cardNumber = cardNumber.Replace(" ", string.Empty);
            // Visa
            if (Regex.IsMatch(cardNumber, "^4[0-9]{12}(?:[0-9]{3})?$"))
            {
                return ValidationResult.Success;
            }
            // MasterCard
            if (Regex.IsMatch(cardNumber, "^(?:5[1-5][0-9]{2}|222[1-9]|22[3-9][0-9]|2[3-6][0-9]{2}|27[01][0-9]|2720)[0-9]{12}$"))
            {
                return ValidationResult.Success;
            }
            // American Express
            if (Regex.IsMatch(cardNumber, "^3[47][0-9]{13}$"))
            {
                return ValidationResult.Success;
            }

            ErrorMessage = "Please provide a valid card number.";
            return new ValidationResult(ErrorMessage);
        }
    }

    public class ExpirationDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // it's not a Required  validator
            if (value == null)
            {
                return ValidationResult.Success;
            }

            DateTime expirationDate;
            if (value is DateTime)
            {
                expirationDate = (DateTime)value;
            }
            else
            {
                string dateString = value.ToString();
                if (!DateTime.TryParse(dateString, out expirationDate))
                {
                    return new ValidationResult("The Expiration Date must be a date.");
                }
                expirationDate = new DateTime(expirationDate.Year, expirationDate.Month, DateTime.DaysInMonth(expirationDate.Year, expirationDate.Month));
            }
            if (expirationDate > DateTime.Now)
            {
                return ValidationResult.Success;
            }

            return new ValidationResult("Your credit card has expired.");
        }
    }
}