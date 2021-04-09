using CVGS.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CVGS.Models.Validators;

namespace CVGS.Models
{
    public class UserViewModel
    {
        [Required]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        // Actual Name
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        // Sex
        [Required]
        [Display(Name = "Sex")]
        public int Sex { get; set; }

        // Date of Birth
        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Birth Date")]
        public DateTime BirthDate { get; set; }

        // Send Promotial Emails
        [Display(Name = "Send Promotional Emails")]
        public bool SendPromotionalEmails { get; set; }

        // Favourite Game Categories
        [Display(Name = "Action")]
        public bool ActionChecked { get; set; }

        [Display(Name = "Adventure")]
        public bool AdventureChecked { get; set; }

        [Display(Name = "Role Playing")]
        public bool RolePlayingChecked { get; set; }

        [Display(Name = "Simulation")]
        public bool SimulationChecked { get; set; }

        [Display(Name = "Strategy")]
        public bool StrategyChecked { get; set; }

        [Display(Name = "Puzzle")]
        public bool PuzzleChecked { get; set; }

        // Favorite Platforms
        [Display(Name = "PC")]
        public bool PCChecked { get; set; }

        [Display(Name = "PlayStation")]
        public bool PlayStationChecked { get; set; }

        [Display(Name = "Xbox")]
        public bool XboxChecked { get; set; }

        [Display(Name = "Nintendo")]
        public bool NintendoChecked { get; set; }

        [Display(Name = "Mobile")]
        public bool MobileChecked { get; set; }

        //[Required]
        [Display(Name = "Mailing Address")]
        public string MailingAddress { get; set; }

        //[Required]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; }
    }
    public class RegisterViewModel : UserViewModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [CustomCreditCard]
        [Display(Name = "Credit Card Number")]
        public string CreditCardNumber { get; set; }

        [ExpirationDate]
        [Display(Name = "Expiration Date")]
        public string ExpirationDate { get; set; }
    }

    public class AccountViewModel : UserViewModel
    {
        public string GameCategoryPreference { get; set; }
        public string GamePlatformPreference { get; set; }
        public string SendPromotionalEmailsPreference { get; set; }

        [Display(Name = "Your credit cards")]
        public List<CreditCard> CreditCards { get; set; }
    }

    public class AddressesViewModel
    {
        [Display(Name = "Apt. #")]
        public string MailingAddressApartment { get; set; }
        [Display(Name = "Street #")]
        public string MailingAddressStreetNumber { get; set; }
        [Display(Name = "Street Name")]
        public string MailingAddressStreetName { get; set; }
        [Display(Name = "City")]
        public string MailingAddressCity { get; set; }
        [Display(Name = "Province")]
        public string MailingAddressProvince { get; set; }
        [Display(Name = "Postal Code")]
        [RegularExpression(@"^[A-Za-z]\d[A-Za-z][ -]?\d[A-Za-z]\d$", ErrorMessage = "Please enter a valid postal code")]
        public string MailingAddressPostalCode { get; set; }

        [Display(Name = "Is your shipping address the same as mailing?")]
        public bool ShippingAddressSame { get; set; }

        [Display(Name = "Apt. #")]
        public string ShippingAddressApartment { get; set; }
        [Display(Name = "Street #")]
        public string ShippingAddressStreetNumber { get; set; }
        [Display(Name = "Street Name")]
        public string ShippingAddressStreetName { get; set; }
        [Display(Name = "City")]
        public string ShippingAddressCity { get; set; }
        [Display(Name = "Province")]
        public string ShippingAddressProvince { get; set; }
        [Display(Name = "Postal Code")]
        [RegularExpression(@"^[A-Za-z]\d[A-Za-z][ -]?\d[A-Za-z]\d$", ErrorMessage = "Please enter a valid postal code")]
        public string ShippingAddressPostalCode { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Confirm email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    #region preinstalled models we don't use
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Display Name")]
        //[EmailAddress]
        public string DisplayName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
    #endregion
}
