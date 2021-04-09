using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using CVGS.Data;
using Microsoft.AspNet.Identity;

namespace CVGS.Controllers
{
    public class WishListsController : Controller
    {
        private CVGSAppEntities db = new CVGSAppEntities();

        public ActionResult Index()
        {
            CVGSAppEntities db = new CVGSAppEntities();
            var userId = User.Identity.GetUserId();
            List<string> gameIds = db.WishLists.Where(w => w.userId == userId).Select(w => w.gameId).ToList();

            var games = db.Games.Where(g => gameIds.Contains(g.Id));
            var categories = db.GameCategories.ToDictionary(item => item.Id, item => item.Name);
            var platforms = db.GamePlatforms.ToDictionary(item => item.Id, item => item.Name);

            foreach (var game in games)
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

            return View(games);
        }

        public async Task<ActionResult> Add(string id)
        {
            var userId = User.Identity.GetUserId();

            if (db.WishLists.Any(c => c.userId == userId && c.gameId == id))
            {
                TempData["message"] = "This game is already in your wish list.";
                return RedirectToAction("ViewGame", new RouteValueDictionary(new { controller = "Home", action = "ViewGame", Id = id }));
            }

            var newWish = new WishList() { userId = userId, gameId = id };
            db.WishLists.Add(newWish);
            await db.SaveChangesAsync();
            return RedirectToAction("ViewGame", new RouteValueDictionary(new { controller = "Home", action = "ViewGame", Id = id }));
        }

        public async Task<ActionResult> Remove(string id)
        {
            var userId = User.Identity.GetUserId();

            if (!db.WishLists.Any(c => c.userId == userId && c.gameId == id))
            {
                TempData["message"] = "This game is not in your wish list.";
                return RedirectToAction("ViewGame", new RouteValueDictionary(new { controller = "Home", action = "ViewGame", Id = id }));
            }

            WishList wish = db.WishLists.FirstOrDefault(c => c.userId == userId && c.gameId == id);
            db.WishLists.Remove(wish);
            await db.SaveChangesAsync();
            return RedirectToAction("ViewGame", new RouteValueDictionary(new { controller = "Home", action = "ViewGame", Id = id }));
        }
    }
}
