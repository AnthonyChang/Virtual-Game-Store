using CVGS.Data;
using CVGS.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace CVGS.Controllers
{
    public class CartController : Controller
    {
        private readonly CVGSAppEntities db;

        public CartController()
        {
            if (db == null)
            {
                db = new CVGSAppEntities();
            }
        }



        // GET: Cart
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new CartViewModel();

            // add games
            List<Game> games = GetGamesFromCart(userId);
            model.GamesInCart = games;

            //calculate total
            model.Total = games.Sum(g => g.Price);

            return View(model);
        }

        public async Task<ActionResult> Add(string id)
        {
            var userId = User.Identity.GetUserId();

            if (db.UserCartItems.Any(c => c.UserId == userId && c.GameId == id))
            {
                // add displaying errors to the home page
                //return RedirectToAction("ViewGame", "Home", new { area = "" });
                TempData["message"] = "This game is already in your cart.";
                return RedirectToAction("ViewGame", new RouteValueDictionary(new { controller = "Home", action = "ViewGame", Id = id }));
            }

            var newCartItem = new UserCartItem() { UserId = userId, GameId = id };
            db.UserCartItems.Add(newCartItem);
            await db.SaveChangesAsync();
            Session["cart"] = db.UserCartItems.Where(c => c.UserId == userId).Count().ToString();
            return RedirectToAction("ViewGame", new RouteValueDictionary(new { controller = "Home", action = "ViewGame", Id = id }));
        }

        [HttpPost]
        public JsonResult RemoveItem(string gameId)
        {
            var userId = User.Identity.GetUserId();

            var gameInCart = db.UserCartItems.FirstOrDefault(c => c.UserId == userId && c.GameId == gameId);
            if (gameInCart != null)
            {
                db.UserCartItems.Remove(gameInCart);
                db.SaveChanges();
                Session["cart"] = db.UserCartItems.Where(c => c.UserId == userId).Count().ToString();
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Success = false, Error = "Error occurred while removing game from your cart" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult MoveToWishList(string gameId)
        {
            var userId = User.Identity.GetUserId();

            var gameInCart = db.UserCartItems.FirstOrDefault(g => g.UserId == userId && g.GameId == gameId);
            if (gameInCart != null)
            {
                var gameInWishList = db.WishLists.FirstOrDefault(g => g.userId == userId && g.gameId == gameId);
                if (gameInWishList != null)
                {
                    return Json(new { Success = false, Error = "This game is already in your wish list" }, JsonRequestBehavior.AllowGet);
                }

                db.WishLists.Add(new WishList { userId = userId, gameId = gameInCart.GameId });
                db.UserCartItems.Remove(gameInCart);
                db.SaveChanges();

                Session["cart"] = db.UserCartItems.Where(c => c.UserId == userId).Count().ToString();
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Success = false, Error = "Error occurred while moving game to your wish list" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ClearAll()
        {
            var userId = User.Identity.GetUserId();

            var gamesInCart = db.UserCartItems.Where(c => c.UserId == userId);
            foreach (var game in gamesInCart)
            {
                db.UserCartItems.Remove(game);
            }
            db.SaveChanges();
            Session["cart"] = db.UserCartItems.Where(c => c.UserId == userId).Count().ToString();
            return RedirectToAction("Index", "Cart");
        }

        // GET: Checkout
        [HttpGet]
        public ActionResult Checkout()
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new CheckoutViewModel();

            // get games
            List<Game> games = GetGamesFromCart(userId);
            model.GamesInCart = games;

            // get credit cards
            model.CreditCards = db.CreditCards.Where(c => c.HolderId == userId).ToList();

            //calculate total
            double tax = 0.13;
            model.Total = games.Sum(g => g.Price);
            model.TaxTotal = Math.Round(model.Total * (decimal)tax, 2);
            model.TotalWithTaxes = Math.Round(model.Total + model.TaxTotal, 2);

            return View(model);
        }

        // Post: Checkout
        [HttpPost]
        public async Task<ActionResult> Checkout(CheckoutViewModel model)
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrEmpty(model.PaymentMethodId))
            {
                ModelState.AddModelError("PaymentMethodId", "A payment method could not be retrieved.");
                return View(model);
            }

            try
            {
                string orderId = Guid.NewGuid().ToString();
                model.GamesInCart = GetGamesFromCart(userId);

                // add new order record
                var newOrder = new Order
                {
                    Id = orderId,
                    UserId = userId,
                    PurchaseDate = DateTime.Now,
                    CreditCardUsed = model.PaymentMethodId
                };
                db.Orders.Add(newOrder);

                // add new orderItem records
                foreach (var game in model.GamesInCart)
                {
                    db.OrderItems.Add(new OrderItem { OrderId = orderId, GameId = game.Id });
                }

                // if any of the games bought were in the user's wish list, remove them from there
                var userWishList = db.WishLists.Where(i => i.userId == userId);
                foreach (var game in model.GamesInCart)
                {
                    var gameInWishList = userWishList.FirstOrDefault(i => i.gameId == game.Id);
                    if (gameInWishList != null)
                    {
                        db.WishLists.Remove(gameInWishList);
                    }
                }


                // remove all items from user's cart
                var cartItem = db.UserCartItems.Where(c => c.UserId == userId).ToList();
                db.UserCartItems.RemoveRange(cartItem);

                List<string> gameIds = new List<string>();

                foreach(Game g in model.GamesInCart)
                {
                    gameIds.Add(g.Id);
                }

                // remove the games from wishlist
                var wishes = db.WishLists.Where(c => c.userId == userId && gameIds.Contains(c.gameId));
                db.WishLists.RemoveRange(wishes);

                await db.SaveChangesAsync();
                Session["cart"] = "0";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occured while attempting to place your order.");
                return View(model);
            }

            return RedirectToAction("OrderComplete", "Cart");
        }

        //GET: View Order
        [HttpGet]
        public ActionResult ViewOrder(string id)
        {
            var userId = User.Identity.GetUserId();
            var model = new CheckoutViewModel();

            // get lastest order
            var order = db.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return RedirectToAction("Orders", "Home");
            }

            string ccNumber = db.CreditCards.Where(cc => (cc.Id + "") == order.CreditCardUsed).FirstOrDefault().CardNumber;

            if (ccNumber != null)
            {
                model.CreditCardLast4 = ccNumber.Substring(ccNumber.Length - 5);
            }

            model.GamesInCart = GetGamesFromLastOrder(userId, order);

            // calculate total
            double tax = 0.13;
            model.Total = model.GamesInCart.Sum(g => g.Price);
            model.TaxTotal = Math.Round(model.Total * (decimal)tax, 2);
            model.TotalWithTaxes = Math.Round(model.Total + model.TaxTotal, 2);

            return View("OrderComplete", model);
        }


        // GET: OrderComplete
        [HttpGet]
        public ActionResult OrderComplete()
        {
            var userId = User.Identity.GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new CheckoutViewModel();

            // get lastest order
            Order lastestOrder = db.Orders.Where(o => o.UserId == userId).OrderByDescending(o => o.PurchaseDate).FirstOrDefault();

            string ccNumber = db.CreditCards.Where(cc => (cc.Id + "") == lastestOrder.CreditCardUsed).FirstOrDefault().CardNumber;

            if (ccNumber != null)
            {
                model.CreditCardLast4 = ccNumber.Substring(ccNumber.Length - 5);
            }

            model.GamesInCart = GetGamesFromLastOrder(userId, lastestOrder);

            // calculate total
            double tax = 0.13;
            model.Total = model.GamesInCart.Sum(g => g.Price);
            model.TaxTotal = Math.Round(model.Total * (decimal)tax, 2);
            model.TotalWithTaxes = Math.Round(model.Total + model.TaxTotal, 2);

            return View(model);
        }

        private List<Game> GetGamesFromLastOrder(string userId, Order lastestOrder)
        {
            List<OrderItem> orderItems = db.OrderItems.Where(oi => oi.OrderId == lastestOrder.Id).ToList();
            List<string> gameIds = new List<string>();

            foreach (var oi in orderItems)
            {
                gameIds.Add(oi.GameId);
            }

            return db.Games.Where(g => gameIds.Contains(g.Id)).ToList();
        }

        private List<Game> GetGamesFromCart(string userId)
        {
            var gamesInCartDB = db.UserCartItems.Where(c => c.UserId == userId).ToList();
            var gamesInDB = db.Games;
            var games = new List<Game>();

            foreach (var game in gamesInCartDB)
            {
                var dbGame = gamesInDB.FirstOrDefault(g => g.Id == game.GameId);
                if (dbGame != null)
                {
                    games.Add(dbGame);
                }
            }

            return games;
        }
    }
}