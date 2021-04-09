using CVGS.Data;
using CVGS.Enums;
using CVGS.Models;
using CVGS.Models.ModelExtensions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CVGS.Controllers
{
    public class ProfileController : Controller
    {
        [HttpGet]
        public async Task<ActionResult> Details(string id)
        {
            CVGSAppEntities db = new CVGSAppEntities();
            var applicationUserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            ProfileViewModel model;

            // determine if the profile owner is friend of the current user
            bool isInFriendList = false;
            bool isFriendedUser = false;
            if ((System.Web.HttpContext.Current.User != null) && System.Web.HttpContext.Current.User.Identity.IsAuthenticated)
            {
                var userId = User.Identity.GetUserId();
                isFriendedUser = userId == id || db.FriendLists.Any(f => f.userId == id && f.friendId == userId);
                isInFriendList = db.FriendLists.Any(f => f.userId == userId && f.friendId == id);
            }

            try
            {
                var user = await db.MemberUsers.FindAsync(id);
                if (user != null)
                {
                    // Get user's preferences
                    var categoryPrefs = (GameCategoryOptions)user.CategoryOptions;
                    var platformPrefs = (FavoritePlatforms)user.PlatformOptions;

                    model = new ProfileViewModel
                    {
                        Id = id,
                        DisplayName = applicationUserManager.FindById(id).UserName,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FavoriteCatefories = categoryPrefs.ToString(),
                        FavoritePlatforms = platformPrefs.ToString(),
                        IsInUserFriendList = isInFriendList,
                        UserInProfileFriendList = isFriendedUser,
                        //ActionChecked = categoryPrefs.HasFlag(GameCategoryOptions.Action),
                        //AdventureChecked = categoryPrefs.HasFlag(GameCategoryOptions.Adventure),
                        //RolePlayingChecked = categoryPrefs.HasFlag(GameCategoryOptions.RolePlaying),
                        //SimulationChecked = categoryPrefs.HasFlag(GameCategoryOptions.Simulation),
                        //StrategyChecked = categoryPrefs.HasFlag(GameCategoryOptions.Strategy),
                        //PuzzleChecked = categoryPrefs.HasFlag(GameCategoryOptions.Puzzle),
                        //PCChecked = platformPrefs.HasFlag(FavoritePlatforms.PC),
                        //PlayStationChecked = platformPrefs.HasFlag(FavoritePlatforms.PlayStation),
                        //XboxChecked = platformPrefs.HasFlag(FavoritePlatforms.Xbox),
                        //NintendoChecked = platformPrefs.HasFlag(FavoritePlatforms.Nintendo),
                        //MobileChecked = platformPrefs.HasFlag(FavoritePlatforms.Mobile)
                    };

                    if (isFriendedUser)
                    {
                        // Get wish list games
                        var wishListIds = db.WishLists.Where(u => u.userId == id).Select(i => i.gameId).ToList();
                        var wishListGames = db.Games.Where(g => wishListIds.Contains(g.Id)).ToList();

                        // Get owned games
                        var orderIds = db.Orders.Where(o => o.UserId == id).Select(o => o.Id).ToList();
                        var gameIds = db.OrderItems.Where(oi => orderIds.Contains(oi.OrderId)).Select(oi => oi.GameId).ToList();
                        var purchasedGames = db.Games.Where(g => gameIds.Contains(g.Id)).ToList();

                        model.WishList = wishListGames;
                        model.OwnedGames = purchasedGames;

                        // Attach category and platform to wish list and games list
                        var categories = db.GameCategories.ToDictionary(item => item.Id, item => item.Name);
                        var platforms = db.GamePlatforms.ToDictionary(item => item.Id, item => item.Name);
                        foreach (var game in model.WishList)
                        {
                            if (categories.Any())
                            {
                                game.CategoryName = game.CategoryId == null ? "None" : categories[(int)game.CategoryId];
                            }
                            if (platforms.Any())
                            {
                                game.PlatformName = game.PlatformId == null ? "None" : platforms[(int)game.PlatformId];
                            }
                        }
                        foreach (var game in model.OwnedGames)
                        {
                            if (categories.Any())
                            {
                                game.CategoryName = game.CategoryId == null ? "None" : categories[(int)game.CategoryId];
                            }
                            if (platforms.Any())
                            {
                                game.PlatformName = game.PlatformId == null ? "None" : platforms[(int)game.PlatformId];
                            }
                        }
                    }
                }
                else
                {
                    return RedirectToAction("FriendList", "Manage");
                }
            }
            catch (Exception)
            {
                TempData["message"] = "Unexpected error occurred while retrieving user profile.";
                return RedirectToAction("FriendList", "Manage");
            }

            return View(model);
        }

        public ActionResult AddToFriendList(string id)
        {
            CVGSAppEntities db = new CVGSAppEntities();
            var userId = User.Identity.GetUserId();

            if (db.FriendLists.FirstOrDefault(f => f.userId == userId && f.friendId == id) == null)
            {
                db.FriendLists.Add(new FriendList { userId = userId, friendId = id });
                db.SaveChanges();
            }
            return RedirectToAction("Details", new { id = id });
        }

        public ActionResult Unfriend(string id)
        {
            CVGSAppEntities db = new CVGSAppEntities();
            var userId = User.Identity.GetUserId();

            var friend = db.FriendLists.FirstOrDefault(f => f.userId == userId && f.friendId == id);
            if (friend != null)
            {
                db.FriendLists.Remove(friend);
                db.SaveChanges();
            }
            return RedirectToAction("FriendList", "Manage");
        }
    }
}
