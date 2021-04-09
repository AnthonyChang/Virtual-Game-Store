using CVGS.Data;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CVGS.Areas.Admin.Controllers
{
    public class ReportController : Controller
    {
        private ApplicationUserManager _userManager;
        private CVGSAppEntities _db;
        private string head = "<head><style>table, th, td { border:1px solid black; border-collapse:collapse; } th { background-color:#ADD8E6; } th, td { padding: 10px; }</style></head>";

        public ReportController()
        {
            if (_db == null)
            {
                _db = new CVGSAppEntities();
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }
        // GET: Admin/Report
        public ActionResult Index()
        {
            return View();
        }

        #region Game List Report
        // GET: Admin/Report/GameList
        public async Task<ActionResult> GameList()
        {
            string subject = "Game List Report";
            string body = generateGameListReport(subject);
            if (string.IsNullOrEmpty(body))
            {
                TempData["error"] = "Unexpected error occurred while generating report";
                return View("Index");
            }

            await UserManager.SendEmailAsync("9f70d4e3-e3dc-45ab-bec2-d7d831b3ca4d", subject, body);
            TempData["message"] = $"The {subject} has been generated and emailed to you.";
            return View("Index");
        }

        private string generateGameListReport(string reportName)
        {
            string today = DateTime.Today.ToString("MM/dd/yyyy");

            string content = head + $"<body><h2>{reportName}</h2><p>Generated on {today}</p>";
            content += "<table><tr><th>Title</th><th>Release Year</th><th>Category</th><th>Platform</th><th>Rating</th><th>Price</th></tr>";

            try
            {
                var games = _db.Games;
                var categories = _db.GameCategories.ToDictionary(item => item.Id, item => item.Name);
                var platforms = _db.GamePlatforms.ToDictionary(item => item.Id, item => item.Name);
                foreach (var game in games)
                {
                    var gameCat = game.CategoryId == null ? "None" : categories[(int)game.CategoryId];
                    var gamePlatform = game.PlatformId == null ? "None" : platforms[(int)game.PlatformId];
                    content += $"<tr><td>{game.Title}</td><td>{game.ReleaseYear}</td><td>{gameCat}</td><td>{gamePlatform}</td>"
                        + $"<td>{game.Rating}</td><td>${game.Price}</td></tr>";

                }
                content += "</table></body>";

                return content;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Member List Report
        // GET: Admin/Report/MemberList
        public async Task<ActionResult> MemberList()
        {
            string subject = "Member List Report";
            string body = generateMemberListReport(subject);
            if (string.IsNullOrEmpty(body))
            {
                TempData["error"] = "Unexpected error occurred while generating report";
                return View("Index");
            }
            await UserManager.SendEmailAsync("9f70d4e3-e3dc-45ab-bec2-d7d831b3ca4d", subject, body);
            TempData["message"] = $"The {subject} has been generated and emailed to you.";
            return View("Index");
        }

        private string generateMemberListReport(string reportName)
        {
            var applicationUserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string today = DateTime.Today.ToString("MM/dd/yyyy");

            string content = head + $"<body><h2>{reportName}</h2><p>Generated on {today}</p>";
            content += "<table><tr><th>Display Name</th><th>Email Address</th><th>Actual Name</th><th>Sex</th><th>Birth Date</th></tr>";

            try
            {
                var aspNetUsers = applicationUserManager.Users;
                var userMembers = _db.MemberUsers;
                foreach (var user in userMembers)
                {
                    var aspNetUser = aspNetUsers.FirstOrDefault(u => u.Id == user.Id);
                    var sex = user.Sex == 0 ? "Male" : user.Sex == 1 ? "Female" : "Prefer Not to Say";

                    content += $"<tr><td>{aspNetUser.UserName}</td><td>{aspNetUser.Email}</td><td>{user.FirstName + " " + user.LastName}</td>"
                        + $"<td>{sex}</td><td>{user.BirthDate.ToString("MM/dd/yyyy")}</td></tr>";

                }
                content += "</table></body>";

                return content;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Wish Lists Report
        // GET: Admin/Report/WishList
        public async Task<ActionResult> WishList()
        {
            string subject = "Wish List Report";
            string body = generateWishListReport(subject);
            if (string.IsNullOrEmpty(body))
            {
                TempData["error"] = "Unexpected error occurred while generating report";
                return View("Index");
            }
            await UserManager.SendEmailAsync("9f70d4e3-e3dc-45ab-bec2-d7d831b3ca4d", subject, body);
            TempData["message"] = $"The {subject} has been generated and emailed to you.";
            return View("Index");
        }

        private string generateWishListReport(string reportName)
        {
            var applicationUserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string today = DateTime.Today.ToString("MM/dd/yyyy");

            string content = head + $"<body><h2>{reportName}</h2><p>Generated on {today}</p>";
            content += "<table><tr><th>Display Name</th><th>Items #</th><th>Average Cost</th><th>Total Cost</th></tr>";

            try
            {
                var aspNetUsers = applicationUserManager.Users;

                foreach (var user in aspNetUsers)
                {
                    string totalCostString = "n/a";
                    string averageCostString = "n/a";
                    var wishList = _db.WishLists.Where(wl => wl.userId == user.Id);
                    if (wishList.Count() != 0)
                    {
                        decimal totalCost = 0;
                        foreach (var game in wishList)
                        {
                            try
                            {
                                totalCost += _db.Games.FirstOrDefault(g => g.Id == game.gameId).Price;
                            }
                            catch { }
                        }
                        totalCostString = "$" + totalCost.ToString();
                        averageCostString = "$" + ((decimal)(totalCost / wishList.Count())).ToString();
                    }

                    content += $"<tr><td>{user.UserName}</td><td>{wishList.Count()}</td><td>{averageCostString}</td>"
                        + $"<td>{totalCostString}</td></tr>";

                }
                content += "</table></body>";

                return content;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Friend Lists Report
        // GET: Admin/Report/WishList
        public async Task<ActionResult> FriendList()
        {
            string subject = "Friend List Report";
            string body = generateFriendListReport(subject);
            if (string.IsNullOrEmpty(body))
            {
                TempData["error"] = "Unexpected error occurred while generating report";
                return View("Index");
            }
            await UserManager.SendEmailAsync("9f70d4e3-e3dc-45ab-bec2-d7d831b3ca4d", subject, body);
            TempData["message"] = $"The {subject} has been generated and emailed to you.";
            return View("Index");
        }

        private string generateFriendListReport(string reportName)
        {
            var applicationUserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string today = DateTime.Today.ToString("MM/dd/yyyy");

            string content = head + $"<body><h2>{reportName}</h2><p>Generated on {today}</p>";
            content += "<table><tr><th>Display Name</th><th>Friends #</th><th>Friended By #</th><th>Mutual Friends #</th></tr>";

            try
            {
                var aspNetUsers = applicationUserManager.Users;
                var friendLists = _db.FriendLists;

                foreach (var user in aspNetUsers)
                {
                    var friends = friendLists.Where(u => u.userId == user.Id);
                    var friendedBy = friendLists.Where(u => u.friendId == user.Id).Select(f => f.userId);
                    var mutualFriends = friends.Where(f => friendedBy.Contains(f.friendId));

                    content += $"<tr><td>{user.UserName}</td><td>{friends.Count()}</td><td>{friendedBy.Count()}</td>"
                        + $"<td>{mutualFriends.Count()}</td></tr>";

                }
                content += "</table></body>";

                return content;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Game Sales Report
        // GET: Admin/Report/GameSales
        public async Task<ActionResult> GameSales()
        {
            string subject = "Game Sales Report";
            string body = generateGameSalesReport(subject);
            if (string.IsNullOrEmpty(body))
            {
                TempData["error"] = "Unexpected error occurred while generating report";
                return View("Index");
            }

            await UserManager.SendEmailAsync("9f70d4e3-e3dc-45ab-bec2-d7d831b3ca4d", subject, body);
            TempData["message"] = "The Game Sales Report has been generated and emailed to you.";
            return View("Index");
        }

        private string generateGameSalesReport(string reportName)
        {
            string today = DateTime.Today.ToString("MM/dd/yyyy");

            string content = head + $"<body><h2>{reportName}</h2><p>Generated on {today}</p>";
            content += "<table><tr><th>Title</th><th>Category</th><th>Platform</th><th>Rating</th><th>Price</th><th># Sales</th><th>Sales Total<th></tr>";

            try
            {
                var games = _db.Games;
                var categories = _db.GameCategories.ToDictionary(item => item.Id, item => item.Name);
                var platforms = _db.GamePlatforms.ToDictionary(item => item.Id, item => item.Name);
                var orderItems = _db.OrderItems;
                foreach (var game in games)
                {
                    var gameCat = game.CategoryId == null ? "None" : categories[(int)game.CategoryId];
                    var gamePlatform = game.PlatformId == null ? "None" : platforms[(int)game.PlatformId];
                    var nbrSold = orderItems.Where(oi => oi.GameId == game.Id).Count();
                    var gameTotal = nbrSold == 0 ? "n/a" : "$" + (nbrSold * game.Price).ToString();
                    content += $"<tr><td>{game.Title}</td><td>{game.ReleaseYear}</td><td>{gameCat}</td><td>{gamePlatform}</td>"
                        + $"<td>{game.Rating}</td><td>${game.Price}</td><td>{nbrSold}</td><td>{gameTotal}</td></tr>";

                }
                content += "</table></body>";

                return content;
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Member Sales Report
        // GET: Admin/Report/GameSales
        public async Task<ActionResult> MemberSales()
        {
            string subject = "Member Sales Report";
            string body = generateMemberSalesReport(subject);
            if (string.IsNullOrEmpty(body))
            {
                TempData["error"] = "Unexpected error occurred while generating report";
                return View("Index");
            }

            await UserManager.SendEmailAsync("9f70d4e3-e3dc-45ab-bec2-d7d831b3ca4d", subject, body);
            TempData["message"] = $"The {subject} has been generated and emailed to you.";
            return View("Index");
        }

        private string generateMemberSalesReport(string reportName)
        {
            var applicationUserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string today = DateTime.Today.ToString("MM/dd/yyyy");

            string content = head + $"<body><h2>{reportName}</h2><p>Generated on {today}</p>";
            content += "<table><tr><th>Display Name</th><th>Email Address</th><th>Actual Name</th><th># Purchases</th><th>Total</th></tr>";

            try
            {
                var aspNetUsers = applicationUserManager.Users;
                var userMembers = _db.MemberUsers;

                var games = _db.Games;
                var orders = _db.Orders;
                var orderItems = _db.OrderItems;
                foreach (var user in userMembers)
                {
                    var aspNetUser = aspNetUsers.FirstOrDefault(u => u.Id == user.Id);
                    var userOrders = orders.Where(o => o.UserId == user.Id);
                    List<OrderItem> userOrderItems = new List<OrderItem>();
                    foreach (var order in userOrders)
                    {
                        userOrderItems.AddRange(orderItems.Where(oi => oi.OrderId == order.Id).ToList());
                    }

                    var nbrSold = userOrderItems.Count();
                    decimal total = 0;
                    if (nbrSold != 0)
                    {
                        foreach (var gameId in userOrderItems.Select(i => i.GameId).Distinct())
                        {
                            total += userOrderItems.Where(i => i.GameId == gameId).Count() * games.FirstOrDefault(g => g.Id == gameId).Price;
                        }
                    }

                    content += $"<tr><td>{aspNetUser.UserName}</td><td>{aspNetUser.Email}</td><td>{user.FirstName + " " + user.LastName}</td>"
                        + $"<td>{nbrSold}</td><td>${total}</td></tr>";

                }
                content += "</table></body>";

                return content;
            }
            catch
            {
                return null;
            }
        }
        #endregion
    }
}