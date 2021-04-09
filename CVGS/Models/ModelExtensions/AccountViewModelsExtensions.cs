using CVGS.Data;
using CVGS.Enums;
using System;

namespace CVGS.Models.ModelExtensions
{
    public static class AccountViewModelsExtensions
    {
        public static MemberUser RegisterViewModelToUser(this RegisterViewModel registerViewModel)
        {
            int categorySelection = 0;
            categorySelection += Convert.ToInt32(registerViewModel.ActionChecked) * (int)GameCategoryOptions.Action
                + Convert.ToInt32(registerViewModel.AdventureChecked) * (int)GameCategoryOptions.Adventure
                + Convert.ToInt32(registerViewModel.RolePlayingChecked) * (int)GameCategoryOptions.RolePlaying
                + Convert.ToInt32(registerViewModel.SimulationChecked) * (int)GameCategoryOptions.Simulation
                + Convert.ToInt32(registerViewModel.StrategyChecked) * (int)GameCategoryOptions.Strategy
                + Convert.ToInt32(registerViewModel.PuzzleChecked) * (int)GameCategoryOptions.Puzzle;

            int platformSelection = 0;
            platformSelection += Convert.ToInt32(registerViewModel.PCChecked) * (int)FavoritePlatforms.PC
                + Convert.ToInt32(registerViewModel.PlayStationChecked) * (int)FavoritePlatforms.PlayStation
                + Convert.ToInt32(registerViewModel.XboxChecked) * (int)FavoritePlatforms.Xbox
                + Convert.ToInt32(registerViewModel.NintendoChecked) * (int)FavoritePlatforms.Nintendo
                + Convert.ToInt32(registerViewModel.MobileChecked) * (int)FavoritePlatforms.Mobile;

            MemberUser user = new MemberUser()
            {

                FirstName =  registerViewModel.FirstName,
                LastName = registerViewModel.LastName,
                Sex = registerViewModel.Sex,
                BirthDate = registerViewModel.BirthDate,
                SendPromotionalEmails = registerViewModel.SendPromotionalEmails,
                CategoryOptions = categorySelection,
                PlatformOptions = platformSelection,
                //MailingAddress = registerViewModel.MailingAddress,
                //ShippingAddress = registerViewModel.ShippingAddress
            };
            return user;
        }

        public static AddressesViewModel MemberUserToAddressesViewModel(this MemberUser memberUser)
        {
            var addresses = new AddressesViewModel
            {
                MailingAddressApartment = memberUser.MailingAddressApartment ?? string.Empty,
                MailingAddressStreetNumber = memberUser.MailingAddressStreetNumber ?? string.Empty,
                MailingAddressStreetName = memberUser.MailingAddressStreetName ?? string.Empty,
                MailingAddressCity = memberUser.MailingAddressCity ?? string.Empty,
                MailingAddressProvince = memberUser.MailingAddressProvince ?? string.Empty,
                MailingAddressPostalCode = memberUser.MailingAddressPostalCode ?? string.Empty,
                ShippingAddressApartment = memberUser.ShippingAddressApartment ?? string.Empty,
                ShippingAddressStreetNumber = memberUser.ShippingAddressStreetNumber ?? string.Empty,
                ShippingAddressStreetName = memberUser.ShippingAddressStreetName ?? string.Empty,
                ShippingAddressCity = memberUser.ShippingAddressCity ?? string.Empty,
                ShippingAddressProvince = memberUser.ShippingAddressProvince ?? string.Empty,
                ShippingAddressPostalCode = memberUser.ShippingAddressPostalCode ?? string.Empty,
                ShippingAddressSame = memberUser.MailingAddressApartment == memberUser.ShippingAddressApartment
                    && memberUser.MailingAddressStreetNumber == memberUser.ShippingAddressStreetNumber
                    && memberUser.MailingAddressStreetName == memberUser.ShippingAddressStreetName
                    && memberUser.MailingAddressCity == memberUser.ShippingAddressCity
                    && memberUser.MailingAddressProvince == memberUser.ShippingAddressProvince ? true : false

            };
            return addresses;
        }
    }
}