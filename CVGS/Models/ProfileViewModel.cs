using CVGS.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CVGS.Models
{
    public class ProfileViewModel
    {
        public string Id { get; set; }

        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        // Actual Name
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Favorite Categories")]
        public string FavoriteCatefories { get; set; }

        //// Favourite Game Categories
        //[Display(Name = "Action")]
        //public bool ActionChecked { get; set; }

        //[Display(Name = "Adventure")]
        //public bool AdventureChecked { get; set; }

        //[Display(Name = "Role Playing")]
        //public bool RolePlayingChecked { get; set; }

        //[Display(Name = "Simulation")]
        //public bool SimulationChecked { get; set; }

        //[Display(Name = "Strategy")]
        //public bool StrategyChecked { get; set; }

        //[Display(Name = "Puzzle")]
        //public bool PuzzleChecked { get; set; }

        [Display(Name = "Favorite Platforms")]
        public string FavoritePlatforms { get; set; }

        //// Favourite Platforms
        //[Display(Name = "PC")]
        //public bool PCChecked { get; set; }

        //[Display(Name = "PlayStation")]
        //public bool PlayStationChecked { get; set; }

        //[Display(Name = "Xbox")]
        //public bool XboxChecked { get; set; }

        //[Display(Name = "Nintendo")]
        //public bool NintendoChecked { get; set; }

        //[Display(Name = "Mobile")]
        //public bool MobileChecked { get; set; }

        public bool IsInUserFriendList { get; set; }

        public bool UserInProfileFriendList { get; set; }

        public List<Game> WishList { get; set; }

        public List<Game> OwnedGames { get; set; }
    }
}