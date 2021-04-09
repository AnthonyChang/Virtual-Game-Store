using System;
using CVGS.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CVGS.Areas.Admin.Controllers
{

    public class GameController : Controller
    {
        const string ADMIN_ROLE = "Employee";

        // GET: Admin/Game
        [CustomAuthorize(Roles = ADMIN_ROLE)]
        public ActionResult Index()
        {
            CVGSAppEntities db = new CVGSAppEntities();
            var games = db.Games;
            var categories = db.GameCategories.ToDictionary(item => item.Id, item => item.Name);
            var platforms = db.GamePlatforms.ToDictionary(item => item.Id, item => item.Name);
            foreach (var game in games)
            {
                game.CategoryName = game.CategoryId == null ? "None" : categories[(int)game.CategoryId];
                game.PlatformName = game.PlatformId == null ? "None" : platforms[(int)game.PlatformId];
            }
            return View(games.ToList());
        }

        // GET: Admin/Game/Create
        [HttpGet]
        [CustomAuthorize(Roles = ADMIN_ROLE)]
        public ActionResult Create()
        {
            using (CVGSAppEntities db = new CVGSAppEntities())
            {
                var gameCategories = new SelectList(db.GameCategories.ToList(), "Id", "Name");
                ViewData["GameCategories"] = gameCategories;
                var gamePlatforms = new SelectList(db.GamePlatforms.ToList(), "Id", "Name");
                ViewData["GamePlatforms"] = gamePlatforms;
            }
            return View();
        }

        // POST: Admin/Game/Create
        [HttpPost]
        [CustomAuthorize(Roles = ADMIN_ROLE)]
        public async Task<ActionResult> Create(Game model)
        {
            model.Rating = 0;
            CVGSAppEntities db = new CVGSAppEntities();
            try
            {
                //Game name unique
                if (db.Games.Any(g => g.Title == model.Title))
                {
                    ModelState.AddModelError("Title", "A game with this name already exists.");
                }

                // Release Year is integer between 1985 and 2020
                if (model.ReleaseYear != null)
                {
                    if (model.ReleaseYear != (int)model.ReleaseYear)
                    {
                        ModelState.AddModelError("ReleaseYear", "The release year must be an integer.");
                    }
                    else if ((int)model.ReleaseYear < 1985 || (int)model.ReleaseYear > 2020)
                    {
                        ModelState.AddModelError("ReleaseYear", "The release year must be between 1985 and 2020.");
                    }
                }
                if (!ModelState.IsValid)
                {
                    var gameCategories = new SelectList(db.GameCategories.ToList(), "Id", "Name");
                    ViewData["GameCategories"] = gameCategories;
                    var gamePlatforms = new SelectList(db.GamePlatforms.ToList(), "Id", "Name");
                    ViewData["GamePlatforms"] = gamePlatforms;
                    return View(model);
                }
                model.Id = Guid.NewGuid().ToString();
                db.Games.Add(model);
                await db.SaveChangesAsync();
                TempData["message"] = $"Record for '{model.Title}' successfully added";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.GetBaseException().Message}");
                var gameCategories = new SelectList(db.GameCategories.ToList(), "Id", "Name");
                ViewData["GameCategories"] = gameCategories;
                var gamePlatforms = new SelectList(db.GamePlatforms.ToList(), "Id", "Name");
                ViewData["GamePlatforms"] = gamePlatforms;
                return View(model);
            }
        }

        // GET: Admin/Game/Edit/{id}
        [HttpGet]
        [CustomAuthorize(Roles = ADMIN_ROLE)]
        public async Task<ActionResult> Edit(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return RedirectToAction(nameof(Index));
            }
            CVGSAppEntities db = new CVGSAppEntities();
            try
            {
                var model = await db.Games.FindAsync(Id);
                if (model == null)
                {
                    TempData["message"] = string.Format("Game record with {0} id was not found", Id);
                    RedirectToAction(nameof(Index));
                }
                var gameCategories = new SelectList(db.GameCategories.ToList(), "Id", "Name");
                ViewData["GameCategories"] = gameCategories;
                var gamePlatforms = new SelectList(db.GamePlatforms.ToList(), "Id", "Name");
                ViewData["GamePlatforms"] = gamePlatforms;
                return View(model);
            }
            catch
            {
                TempData["message"] = string.Format("Unexpected error occurred while updating a game (id = {0})", Id);
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Game/Edit/{id}
        [HttpPost]
        [CustomAuthorize(Roles = ADMIN_ROLE)]
        public async Task<ActionResult> Edit(Game model)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                return RedirectToAction(nameof(Index));
            }
            CVGSAppEntities db = new CVGSAppEntities();
            try
            {
                // Unique Name
                if (db.Games.Any(e => e.Title == model.Title && e.Id != model.Id))
                {
                    ModelState.AddModelError("Title", "An event with this name already exists.");
                }
                // Release Year is integer between 1985 and 2020
                if (model.ReleaseYear != null)
                {
                    if (model.ReleaseYear != (int)model.ReleaseYear)
                    {
                        ModelState.AddModelError("ReleaseYear", "The release year must be an integer.");
                    }
                    else if ((int)model.ReleaseYear < 1985 || (int)model.ReleaseYear > 2020)
                    {
                        ModelState.AddModelError("ReleaseYear", "The release year must be between 1985 and 2020.");
                    }
                }
                if (!ModelState.IsValid)
                {
                    var gameCategories = new SelectList(db.GameCategories.ToList(), "Id", "Name");
                    ViewData["GameCategories"] = gameCategories;
                    var gamePlatforms = new SelectList(db.GamePlatforms.ToList(), "Id", "Name");
                    ViewData["GamePlatforms"] = gamePlatforms;
                    return View(model);
                }

                var game = await db.Games.FindAsync(model.Id);
                if (game == null)
                {
                    TempData["message"] = string.Format("Game record with {0} id was not found", model.Id);
                    RedirectToAction(nameof(Index));
                }
                game.Title = model.Title;
                game.Description = model.Description;
                game.ReleaseYear = model.ReleaseYear;
                game.CategoryId = model.CategoryId;
                game.PlatformId = model.PlatformId;
                game.Price = model.Price;
                game.ImageUrl = model.ImageUrl;
                await db.SaveChangesAsync();
                TempData["message"] = $"Record for '{model.Title}' successfully updated";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.GetBaseException().Message}");
                var gameCategories = new SelectList(db.GameCategories.ToList(), "Id", "Name");
                ViewData["GameCategories"] = gameCategories;
                var gamePlatforms = new SelectList(db.GamePlatforms.ToList(), "Id", "Name");
                ViewData["GamePlatforms"] = gamePlatforms;
                return View(model);
            }
        }

        // GET: Admin/Game/Delete/{id}
        [HttpGet]
        [CustomAuthorize(Roles = ADMIN_ROLE)]
        public async Task<ActionResult> Delete(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return RedirectToAction(nameof(Index));
            }

            CVGSAppEntities db = new CVGSAppEntities();
            var model = await db.Games.FindAsync(Id);
            if (model == null)
            {
                TempData["message"] = string.Format("Game record with {0} id was not found", Id);
                RedirectToAction(nameof(Index));
            }

            var categories = db.GameCategories.ToDictionary(item => item.Id, item => item.Name);
            var platforms = db.GamePlatforms.ToDictionary(item => item.Id, item => item.Name);

            model.CategoryName = model.CategoryId == null ? "None" : categories[(int)model.CategoryId];
            model.PlatformName = model.PlatformId == null ? "None" : platforms[(int)model.PlatformId];

            return View(model);
        }

        // POST: Admin/Game/Delete/{id}
        [HttpPost]
        [CustomAuthorize(Roles = ADMIN_ROLE)]
        public async Task<ActionResult> Delete(Game model)
        {
            CVGSAppEntities db = new CVGSAppEntities();
            if (string.IsNullOrEmpty(model.Id))
            {
                return RedirectToAction(nameof(Index));
            }

            var game = await db.Games.FindAsync(model.Id);
            if (game == null)
            {
                TempData["message"] = string.Format("Game record with {0} id was not found", model.Id);
                RedirectToAction(nameof(Index));
            }
            try
            {
                // remove all ratings for the game
                var gameRatings = db.GameRatings;
                foreach (var rating in gameRatings.Where(g => g.GameId == model.Id))
                {
                    gameRatings.Remove(rating);
                }

                // remove all reviews for the game
                var gameReviews = db.GameReviews;
                foreach (var review in gameReviews.Where(g => g.GameId == model.Id))
                {
                    gameReviews.Remove(review);
                }

                // remove the game from orders
                var orderItems = db.OrderItems;
                foreach (var orderItem in orderItems.Where(g => g.GameId == model.Id))
                {
                    orderItems.Remove(orderItem);
                }

                // remove the game from user carts
                var userCartItems = db.UserCartItems;
                foreach (var userCartItem in userCartItems.Where(g => g.GameId == model.Id))
                {
                    userCartItems.Remove(userCartItem);
                }

                // remove the game from user carts
                var wishListItems = db.WishLists;
                foreach (var item in wishListItems.Where(g => g.gameId == model.Id))
                {
                    wishListItems.Remove(item);
                }

                var title = game.Title;
                db.Games.Attach(game);
                db.Games.Remove(game);
                await db.SaveChangesAsync();
                TempData["message"] = string.Format("Record '{0}' successfully deleted", title);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.GetBaseException().Message}");
                return View(model);
            }
        }
    }
}