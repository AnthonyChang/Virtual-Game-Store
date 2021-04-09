using CVGS.Data;
using CVGS.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CVGS.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            CVGSAppEntities db = new CVGSAppEntities();
            var userId = User.Identity.GetUserId();
            List<string> gameIds = new List<string>();

            IQueryable<Game> model = db.Games.Where(g => !string.IsNullOrEmpty(g.ImageUrl));

            if (TempData.ContainsKey("SearchString"))
            {
                string searchString = (string)TempData["SearchString"];
                if (!string.IsNullOrEmpty(searchString))
                {
                    model = model.Where(g => g.Title.ToLower().Contains(searchString.ToLower()));
                }
            }

            var categories = db.GameCategories.ToDictionary(item => item.Id, item => item.Name);
            var platforms = db.GamePlatforms.ToDictionary(item => item.Id, item => item.Name);
            foreach (Game game in model)
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
            return View(model);
        }

        [HttpGet]
        public ActionResult SearchGames(string search)
        {
            TempData["SearchString"] = search ?? "";
            return RedirectToAction("Index");
        }


        [HttpPost]
        public JsonResult AutoComplete(string term)
        {
            CVGSAppEntities db = new CVGSAppEntities();
            var games = db.Games.ToList();
            if (!string.IsNullOrEmpty(term))
            {
                games = games.Where(g => g.Title.ToLower().Contains(term.ToLower()) && !string.IsNullOrEmpty(g.ImageUrl)).ToList();
            }
            var jsonGames = games.Select(g => new { label = g.Title, value = g.Id }).ToList();

            return Json(jsonGames, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult AutoCompleteUsers(string term)
        {
            var applicationUserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var users = applicationUserManager.Users.ToList();
            if (!string.IsNullOrEmpty(term))
            {
                users = users.Where(g => g.UserName.ToLower().Contains(term.ToLower()) && g.UserName != "Admin").ToList();
            }
            var jsonUsers = users.Select(g => new { label = g.UserName, value = g.Id }).ToList();

            return Json(jsonUsers, JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public JsonResult AddRating(GameRating newRating)
        {
            CVGSAppEntities db = new CVGSAppEntities();

            var userId = User.Identity.GetUserId();

            var existingRatingByUser = db.GameRatings.FirstOrDefault(rating => rating.GameId == newRating.GameId && rating.UserId == userId);

            if (existingRatingByUser != null)
            {
                existingRatingByUser.Rating = newRating.Rating;
            }
            else
            {
                GameRating gameRating = new GameRating
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Rating = newRating.Rating,
                    GameId = newRating.GameId
                };

                db.GameRatings.Add(gameRating);
            }

            db.SaveChanges();

            // Get new average
            var ratings = db.GameRatings.Where(rating => rating.GameId == newRating.GameId);

            decimal averageRating = Convert.ToDecimal(ratings.Average(r => r.Rating));

            var game = db.Games.FirstOrDefault(g => g.Id == newRating.GameId);

            if (game != null)
            {
                game.Rating = averageRating;
                db.SaveChanges();
            }
            return Json(new { Success = true, Rating = averageRating }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Library()
        {
            CVGSAppEntities db = new CVGSAppEntities();
            var userId = User.Identity.GetUserId();
            var categories = db.GameCategories.ToDictionary(item => item.Id, item => item.Name);
            var platforms = db.GamePlatforms.ToDictionary(item => item.Id, item => item.Name);
            var purchasedGames = GetPurchaseDGames(db, userId);

            foreach (var game in purchasedGames)
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

            return View(purchasedGames);
        }

        private IQueryable<Game> GetPurchaseDGames(CVGSAppEntities db, string userId)
        {
            var orderIds = db.Orders.Where(o => o.UserId == userId).Select(o => o.Id).ToList();
            var gameIds = db.OrderItems.Where(oi => orderIds.Contains(oi.OrderId)).Select(oi => oi.GameId).ToList();
            IQueryable<Game> purchasedGames = db.Games.Where(g => gameIds.Contains(g.Id));
            return purchasedGames;
        }

        public ActionResult Orders()
        {
            CVGSAppEntities db = new CVGSAppEntities();
            var userId = User.Identity.GetUserId();

            var orders = db.Orders.Where(o => o.UserId == userId);
            var orderItems = db.OrderItems;
            var games = db.Games;

            List<OrdersViewModel> model = new List<OrdersViewModel>();

            foreach (var order in orders)
            {
                var gameIds = db.OrderItems.Where(oi => oi.OrderId == order.Id).Select(oi => oi.GameId).ToList();

                var orderGames = games.Where(g => gameIds.Contains(g.Id));
                //decimal total = 0;
                var total = Math.Round(orderGames.Sum(g => g.Price) * (decimal)1.13, 2);
                List<GameThumbView> gameThumbs = orderGames.Select(g => new GameThumbView { Id = g.Id, ImageUrl = g.ImageUrl }).ToList();

                model.Add(new OrdersViewModel
                {
                    Id = order.Id,
                    OrderPlaced = order.PurchaseDate,
                    ItemNumber = orderItems.Where(oi => oi.OrderId == order.Id).Count(),
                    Total = total,
                    Games = gameThumbs
                });
            }

            return View(model);
        }

        #region Actions rendering a game
        [HttpGet]
        public async Task<ActionResult> ViewGame(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return RedirectToAction(nameof(Index));
            }

            CVGSAppEntities db = new CVGSAppEntities();

            try
            {
                var game = await db.Games.FindAsync(Id);
                if (game == null)
                {
                    RedirectToAction(nameof(Index));
                }
                var gameCategory = db.GameCategories.FirstOrDefault(g => g.Id == game.CategoryId);
                game.CategoryName = gameCategory == null ? "None" : gameCategory.Name;
                var gamePlatform = db.GamePlatforms.FirstOrDefault(g => g.Id == game.PlatformId);
                game.PlatformName = gamePlatform == null ? "None" : gamePlatform.Name;

                var model = new GameViewModel(game);

                // add reviews for the game
                var reviews = db.GameReviews.Where(r => r.GameId == Id && r.Approved == true);
                var gameReviews = new List<ReviewViewModel>();
                var applicationUserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
                foreach (var review in reviews)
                {
                    gameReviews.Add(new ReviewViewModel
                    {
                        Id = review.Id,
                        ReviewerId = review.CreatedBy,
                        Reviewer = applicationUserManager.FindById(review.CreatedBy).UserName,
                        EditedOn = review.EditedOn,
                        ReviewText = review.ReviewText
                    });
                }
                model.Reviews = new ReviewListModel();
                model.Reviews.GameId = Id;
                model.Reviews.ApprovedReviews = gameReviews.OrderBy(r => r.EditedOn).ToList();

                // add unapproved reviews for the game by current user
                var userId = User.Identity.GetUserId();
                var userName = User.Identity.GetUserName();
                var unapprovedReviews = db.GameReviews
                    .Where(r => r.GameId == Id && r.CreatedBy == userId && r.Approved == false)
                    .Select(r => new ReviewViewModel
                    {
                        Id = r.Id,
                        ReviewerId = r.Id,
                        Reviewer = userName,
                        ReviewText = r.ReviewText,
                        EditedOn = r.EditedOn
                    })
                    .ToList();

                model.Reviews.UnapprovedReviewsByUser = unapprovedReviews.OrderBy(r => r.EditedOn).ToList();

                // determine whether the game is already in the shopping cart
                model.IsInUserCart = db.UserCartItems.Any(c => c.UserId == userId && c.GameId == Id);

                // detemine whether the game is already purchased
                List<string> allOrderIds = new List<string>();
                var allOrders = db.Orders.Where(o => o.UserId == userId);

                foreach (Order o in allOrders)
                {
                    allOrderIds.Add(o.Id);
                }

                model.IsPurchased = db.OrderItems.Any(oi => oi.GameId == Id && allOrderIds.Contains(oi.OrderId));

                // Determine if game is in wishlist
                model.IsInWishList = db.WishLists.Any(wish => wish.gameId == Id && wish.userId == userId);

                return View(model);
            }
            catch (Exception e)
            {
                TempData["message"] = "Unexpected error occurred while accessing the game";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        //[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult _ReviewListPartial(string Id)
        {
            // TODO: Validate ID!

            CVGSAppEntities db = new CVGSAppEntities();

            var model = new ReviewListModel();
            model.GameId = Id;

            // add approved reviews for the game
            var reviews = db.GameReviews.Where(r => r.GameId == Id && r.Approved == true);
            var approvedReviews = new List<ReviewViewModel>();
            var applicationUserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            foreach (var review in reviews)
            {
                approvedReviews.Add(new ReviewViewModel
                {
                    Id = review.Id,
                    ReviewerId = review.CreatedBy,
                    Reviewer = applicationUserManager.FindById(review.CreatedBy).UserName,
                    EditedOn = review.EditedOn,
                    ReviewText = review.ReviewText
                });
            }
            model.ApprovedReviews = approvedReviews.OrderBy(m => m.EditedOn).ToList();

            // add unapproved reviews for the game left by the current user
            var userId = User.Identity.GetUserId();
            var userName = User.Identity.GetUserName();
            var unapprovedReviews = db.GameReviews
                .Where(r => r.GameId == Id && r.CreatedBy == userId && r.Approved == false)
                .Select(r => new ReviewViewModel
                {
                    Id = r.Id,
                    ReviewerId = r.Id,
                    Reviewer = userName,
                    ReviewText = r.ReviewText,
                    EditedOn = r.EditedOn
                })
                .ToList();

            model.UnapprovedReviewsByUser = unapprovedReviews.OrderBy(r => r.EditedOn).ToList();
            return PartialView(model);
        }
        #endregion


        [HttpPost]
        public JsonResult DownloadGame(string downloadUrl)
        {
            WebClient webClient = new WebClient();
            String url = @"D:\Temp" + DateTime.Now;
            webClient.DownloadFile(downloadUrl, url);

            return Json(new { Success = true, URL = url }, JsonRequestBehavior.AllowGet);
        }

        #region Review CRUD
        // POST: /Home/AddReview
        [HttpPost]
        public JsonResult AddReview(GameReview review)
        {
            review.Id = Guid.NewGuid().ToString();
            review.CreatedBy = User.Identity.GetUserId();
            review.CreatedOn = DateTime.Now;
            review.EditedOn = DateTime.Now;
            review.Approved = false;

            CVGSAppEntities db = new CVGSAppEntities();
            try
            {
                db.GameReviews.Add(review);
                db.SaveChanges();
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { Success = false, Error = "Error occurred while adding review" }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: /Home/EditReview
        [HttpPost]
        public JsonResult EditReview(GameReview review)
        {
            CVGSAppEntities db = new CVGSAppEntities();
            try
            {
                var memberId = User.Identity.GetUserId();
                var savedReview = db.GameReviews.FirstOrDefault(r => r.Id == review.Id);

                savedReview.ReviewText = review.ReviewText;
                savedReview.EditedOn = DateTime.Now;
                savedReview.Approved = false;

                db.SaveChanges();
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { Success = false, Error = "Error occurred while editing review" }, JsonRequestBehavior.AllowGet);
            }
        }

        // POST: /Home/DeleteReview
        [HttpPost]
        public JsonResult DeleteReview(GameReview review)
        {
            CVGSAppEntities db = new CVGSAppEntities();
            try
            {
                var memberId = User.Identity.GetUserId();
                var savedReview = db.GameReviews.FirstOrDefault(r => r.Id == review.Id);
                db.GameReviews.Remove(savedReview);
                db.SaveChanges();
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { Success = false, Error = "Error occurred while deleting review" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}