using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using CVGS.Models;
using CVGS.Data;
using System.Collections.Generic;
using CVGS.Enums;
using CVGS.Models.ModelExtensions;
using System.Text.RegularExpressions;

namespace CVGS.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
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

        #region account details: view, modify
        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : message == ManageMessageId.SavedAccountDetailsSuccess ? "Your new account information saved successfully."
                : "";

            var userId = User.Identity.GetUserId();
            var memberUser = new MemberUser();
            var creditCards = new List<CreditCard>();
            CVGSAppEntities db = new CVGSAppEntities();

            memberUser = db.MemberUsers.FirstOrDefault(m => m.Id == userId);
            creditCards = db.CreditCards.Where(c => c.HolderId == userId).ToList();

            var categoryPrefs = (GameCategoryOptions)memberUser.CategoryOptions;
            var platformPrefs = (FavoritePlatforms)memberUser.PlatformOptions;

            var mailingAddress = string.Empty;
            if (!string.IsNullOrEmpty(memberUser.MailingAddressStreetNumber))
            {
                mailingAddress = string.IsNullOrEmpty(memberUser.MailingAddressApartment) ? string.Empty : memberUser.MailingAddressApartment + "-";
                //mailingAddress += string.IsNullOrEmpty(mailingAddress) ? string.Empty : " ";
                mailingAddress += memberUser.MailingAddressStreetNumber;
                mailingAddress += " " + memberUser.MailingAddressStreetName;
                mailingAddress += ", " + memberUser.MailingAddressCity;
                mailingAddress += ", " + db.Provinces.FirstOrDefault(p => p.Code == memberUser.MailingAddressProvince).Name;
                mailingAddress += ", " + memberUser.MailingAddressPostalCode;
            }

            var shippingAddress = string.Empty;
            if (!string.IsNullOrEmpty(memberUser.ShippingAddressStreetNumber))
            {
                shippingAddress = string.IsNullOrEmpty(memberUser.ShippingAddressApartment) ? string.Empty : memberUser.ShippingAddressApartment + "-";
                //shippingAddress += string.IsNullOrEmpty(shippingAddress) ? string.Empty : " ";
                shippingAddress += memberUser.ShippingAddressStreetNumber;
                shippingAddress += " " + memberUser.ShippingAddressStreetName;
                shippingAddress += ", " + memberUser.ShippingAddressCity;
                shippingAddress += ", " + db.Provinces.FirstOrDefault(p => p.Code == memberUser.ShippingAddressProvince).Name;
                shippingAddress += ", " + memberUser.ShippingAddressPostalCode;
            }



            var model = new AccountViewModel
            {
                DisplayName = User.Identity.Name,
                Email = await UserManager.GetEmailAsync(userId),
                FirstName = memberUser.FirstName,
                LastName = memberUser.LastName,
                BirthDate = memberUser.BirthDate,
                Sex = memberUser.Sex == null ? 2 : (int)memberUser.Sex,
                GameCategoryPreference = categoryPrefs.ToString(),
                GamePlatformPreference = platformPrefs.ToString(),
                SendPromotionalEmailsPreference = memberUser.SendPromotionalEmails == null || !(bool)memberUser.SendPromotionalEmails ? "No" : "Yes",
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
                //MobileChecked = platformPrefs.HasFlag(FavoritePlatforms.Mobile),
                //SendPromotionalEmails = memberUser.SendPromotionalEmails == null ? false : (bool)memberUser.SendPromotionalEmails,
                MailingAddress = mailingAddress,
                ShippingAddress = shippingAddress,
                CreditCards = creditCards ?? new List<CreditCard>()
            };
            return View(model);
        }

        //
        // GET: /Manage/ChangeAccountDetails
        public async Task<ActionResult> ChangeAccountDetails()
        {
            var userId = User.Identity.GetUserId();
            var memberUser = new MemberUser();
            using (CVGSAppEntities db = new CVGSAppEntities())
            {
                memberUser = db.MemberUsers.FirstOrDefault(m => m.Id == userId);
            }
            var categoryPrefs = (GameCategoryOptions)memberUser.CategoryOptions;
            var platformPrefs = (FavoritePlatforms)memberUser.PlatformOptions;
            var model = new UserViewModel
            {
                DisplayName = User.Identity.Name,
                Email = await UserManager.GetEmailAsync(userId),
                FirstName = memberUser.FirstName,
                LastName = memberUser.LastName,
                BirthDate = memberUser.BirthDate,
                Sex = memberUser.Sex == null ? 2 : (int)memberUser.Sex,
                ActionChecked = categoryPrefs.HasFlag(GameCategoryOptions.Action),
                AdventureChecked = categoryPrefs.HasFlag(GameCategoryOptions.Adventure),
                RolePlayingChecked = categoryPrefs.HasFlag(GameCategoryOptions.RolePlaying),
                SimulationChecked = categoryPrefs.HasFlag(GameCategoryOptions.Simulation),
                StrategyChecked = categoryPrefs.HasFlag(GameCategoryOptions.Strategy),
                PuzzleChecked = categoryPrefs.HasFlag(GameCategoryOptions.Puzzle),
                PCChecked = platformPrefs.HasFlag(FavoritePlatforms.PC),
                PlayStationChecked = platformPrefs.HasFlag(FavoritePlatforms.PlayStation),
                XboxChecked = platformPrefs.HasFlag(FavoritePlatforms.Xbox),
                NintendoChecked = platformPrefs.HasFlag(FavoritePlatforms.Nintendo),
                MobileChecked = platformPrefs.HasFlag(FavoritePlatforms.Mobile),
                SendPromotionalEmails = memberUser.SendPromotionalEmails == null ? false : (bool)memberUser.SendPromotionalEmails
            };
            return View(model);
        }

        //
        // POST: /Manage/ChangeAccountDetails
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeAccountDetails(UserViewModel model)
        {
            // get user object from the storage
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            // Display name validation
            if (UserManager.FindByName(model.DisplayName) != null && user.UserName != model.DisplayName)
            {
                ModelState.AddModelError("DisplayName", "An account with this display name already exists.");
            }

            // Email validation
            if (UserManager.FindByEmail(model.Email) != null && user.Email != model.Email)
            {
                ModelState.AddModelError("Email", "An account with this email address already exists.");
            }

            // Birth date validation
            if (model.BirthDate > DateTime.Now.AddYears(-18))
            {
                ModelState.AddModelError("BirthDate", "You must be at least 18 years of age.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // change username and email
            user.UserName = model.DisplayName;
            user.Email = model.Email;

            int categorySelection = 0;
            categorySelection += Convert.ToInt32(model.ActionChecked) * (int)GameCategoryOptions.Action
                + Convert.ToInt32(model.AdventureChecked) * (int)GameCategoryOptions.Adventure
                + Convert.ToInt32(model.RolePlayingChecked) * (int)GameCategoryOptions.RolePlaying
                + Convert.ToInt32(model.SimulationChecked) * (int)GameCategoryOptions.Simulation
                + Convert.ToInt32(model.StrategyChecked) * (int)GameCategoryOptions.Strategy
                + Convert.ToInt32(model.PuzzleChecked) * (int)GameCategoryOptions.Puzzle;

            int platformSelection = 0;
            platformSelection += Convert.ToInt32(model.PCChecked) * (int)FavoritePlatforms.PC
                + Convert.ToInt32(model.PlayStationChecked) * (int)FavoritePlatforms.PlayStation
                + Convert.ToInt32(model.XboxChecked) * (int)FavoritePlatforms.Xbox
                + Convert.ToInt32(model.NintendoChecked) * (int)FavoritePlatforms.Nintendo
                + Convert.ToInt32(model.MobileChecked) * (int)FavoritePlatforms.Mobile;

            using (CVGSAppEntities db = new CVGSAppEntities())
            {
                var memberUser = db.MemberUsers.FirstOrDefault(c => c.Id == user.Id);
                memberUser.FirstName = model.FirstName;
                memberUser.LastName = model.LastName;
                memberUser.Sex = model.Sex;
                memberUser.BirthDate = model.BirthDate;
                memberUser.SendPromotionalEmails = model.SendPromotionalEmails;
                memberUser.CategoryOptions = categorySelection;
                memberUser.PlatformOptions = platformSelection;
                db.SaveChanges();
            }

            // Persiste the changes
            var result = await UserManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // this is needed to update cache where variable User is stored
                // (otherwise will diplay old values, even though values in database will be updated correctly)
                // no need to use it when updating our datatable
                await SignInManager.SignInAsync(user, true, rememberBrowser: false);
                return RedirectToAction("Index");
            }
            AddErrors(result);
            return View(model);
        }
        #endregion

        public ActionResult FriendList()
        {
            CVGSAppEntities db = new CVGSAppEntities();
            var applicationUserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var userId = User.Identity.GetUserId();

            var friends = db.FriendLists.Where(f => f.userId == userId);

            List<FriendListView> friendListModel = new List<FriendListView>();
            foreach (var friend in friends)
            {
                var name = applicationUserManager.FindById(friend.friendId).UserName;
                if (!string.IsNullOrEmpty(name))
                {
                    friendListModel.Add(new FriendListView { Id = friend.friendId, DisplayName = name });
                }
            }

            return View(friendListModel);
        }

        [HttpGet]
        public ActionResult SearchUserProfile(string search)
        {
            var applicationUserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var userFound = applicationUserManager.Users.FirstOrDefault(u => u.UserName.ToLower() == search.ToLower());
            if (userFound != null && userFound.UserName != "Admin")
            {
                return RedirectToAction("Details", "Profile", new { id = userFound.Id });
            }
            TempData["message"] = $"No member with display name \"{search}\" found.";
            return RedirectToAction("FriendList", "Manage");
        }

        #region change addresses
        [HttpGet]
        public ActionResult ChangeAddressDetails()
        {
            // get user object from the storage
            var userId = User.Identity.GetUserId();
            //var user = await UserManager.FindByIdAsync(userId);
            CVGSAppEntities db = new CVGSAppEntities();

            var memberUser = db.MemberUsers.FirstOrDefault(m => m.Id == userId);
            var model = memberUser.MemberUserToAddressesViewModel();
            //var provinces = new SelectList(db.Provinces.ToList(), "Code", "Name");
            List<SelectListItem> provinces = db.Provinces
                .OrderBy(p => p.Name)
                .Select(p =>
                    new SelectListItem
                    {
                        Value = p.Code,
                        Text = p.Name
                    }).ToList();

            provinces.Insert(0, new SelectListItem { Text = "", Value = "" });
            ViewData["ProvinceList"] = provinces;
            return View(model);
        }
        [HttpPost]
        public async Task<ActionResult> ChangeAddressDetails(AddressesViewModel model)
        {
            CVGSAppEntities db = new CVGSAppEntities();

            // if only some parts of mailing address are entered
            if ((!string.IsNullOrEmpty(model.MailingAddressApartment) || !string.IsNullOrEmpty(model.MailingAddressStreetNumber) || !string.IsNullOrEmpty(model.MailingAddressStreetName)
                || !string.IsNullOrEmpty(model.MailingAddressCity) || !string.IsNullOrEmpty(model.MailingAddressProvince))
                && (string.IsNullOrEmpty(model.MailingAddressStreetNumber) || string.IsNullOrEmpty(model.MailingAddressStreetName)
                || string.IsNullOrEmpty(model.MailingAddressCity) || string.IsNullOrEmpty(model.MailingAddressProvince)))
            {
                ModelState.AddModelError("", "Please provide a full mailing address or clear all mailing address fields.");
            }
            // if only some parts of shipping address are entered
            if ((!string.IsNullOrEmpty(model.ShippingAddressApartment) || !string.IsNullOrEmpty(model.ShippingAddressStreetNumber) || !string.IsNullOrEmpty(model.ShippingAddressStreetName)
                || !string.IsNullOrEmpty(model.ShippingAddressCity) || !string.IsNullOrEmpty(model.ShippingAddressProvince))
                && (string.IsNullOrEmpty(model.ShippingAddressStreetNumber) || string.IsNullOrEmpty(model.ShippingAddressStreetName)
                || string.IsNullOrEmpty(model.ShippingAddressCity) || string.IsNullOrEmpty(model.ShippingAddressProvince)))
            {
                ModelState.AddModelError("", "Please provide a full shipping address or clear all shipping address fields.");
            }
            if (!ModelState.IsValid)
            {
                List<SelectListItem> provinces = db.Provinces
                .OrderBy(p => p.Name)
                .Select(p =>
                    new SelectListItem
                    {
                        Value = p.Code,
                        Text = p.Name
                    }).ToList();

                provinces.Insert(0, new SelectListItem { Text = "", Value = "" });
                ViewData["ProvinceList"] = provinces;
                return View(model);
            }


            // get member user object
            var userId = User.Identity.GetUserId();
            var memberUser = await db.MemberUsers.FindAsync(userId);

            // format the postal codes
            if (!string.IsNullOrEmpty(model.MailingAddressPostalCode))
            {
                int startingIndexSecondPart = model.MailingAddressPostalCode.Length == 6 ? 3 : 4;
                memberUser.MailingAddressPostalCode = formatPostalCode(model.MailingAddressPostalCode, startingIndexSecondPart);
            }
            else
            {
                memberUser.MailingAddressPostalCode = null;
            }
            if (model.ShippingAddressSame)
            {
                memberUser.ShippingAddressPostalCode = memberUser.MailingAddressPostalCode;
            }
            else if (!string.IsNullOrEmpty(model.ShippingAddressPostalCode))
            {
                int startingIndexSecondPart = model.ShippingAddressPostalCode.Length == 6 ? 3 : 4;
                memberUser.ShippingAddressPostalCode = formatPostalCode(model.ShippingAddressPostalCode, startingIndexSecondPart);
            }
            else
            {
                memberUser.ShippingAddressPostalCode = null;
            }

            // update and save member user record
            memberUser.MailingAddressApartment = !string.IsNullOrEmpty(model.MailingAddressApartment) ? model.MailingAddressApartment.Trim() : string.Empty;
            memberUser.MailingAddressStreetNumber = !string.IsNullOrEmpty(model.MailingAddressStreetNumber) ? model.MailingAddressStreetNumber.Trim() : string.Empty;
            memberUser.MailingAddressStreetName = !string.IsNullOrEmpty(model.MailingAddressStreetName) ? model.MailingAddressStreetName.Trim() : string.Empty;
            memberUser.MailingAddressCity = !string.IsNullOrEmpty(model.MailingAddressCity) ? model.MailingAddressCity.Trim() : string.Empty;
            memberUser.MailingAddressProvince = !string.IsNullOrEmpty(model.MailingAddressProvince) ? model.MailingAddressProvince.Trim() : string.Empty;
            if (model.ShippingAddressSame)
            {
                memberUser.ShippingAddressApartment = !string.IsNullOrEmpty(model.MailingAddressApartment) ? model.MailingAddressApartment.Trim() : string.Empty;
                memberUser.ShippingAddressStreetNumber = !string.IsNullOrEmpty(model.MailingAddressStreetNumber) ? model.MailingAddressStreetNumber.Trim() : string.Empty;
                memberUser.ShippingAddressStreetName = !string.IsNullOrEmpty(model.MailingAddressStreetName) ? model.MailingAddressStreetName.Trim() : string.Empty;
                memberUser.ShippingAddressCity = !string.IsNullOrEmpty(model.MailingAddressCity) ? model.MailingAddressCity.Trim() : string.Empty;
                memberUser.ShippingAddressProvince = !string.IsNullOrEmpty(model.MailingAddressProvince) ? model.MailingAddressProvince.Trim() : string.Empty;
            }
            else
            {
                memberUser.ShippingAddressApartment = !string.IsNullOrEmpty(model.ShippingAddressApartment) ? model.ShippingAddressApartment.Trim() : string.Empty;
                memberUser.ShippingAddressStreetNumber = !string.IsNullOrEmpty(model.ShippingAddressStreetNumber) ? model.ShippingAddressStreetNumber.Trim() : string.Empty;
                memberUser.ShippingAddressStreetName = !string.IsNullOrEmpty(model.ShippingAddressStreetName) ? model.ShippingAddressStreetName.Trim() : string.Empty;
                memberUser.ShippingAddressCity = !string.IsNullOrEmpty(model.ShippingAddressCity) ? model.ShippingAddressCity.Trim() : string.Empty;
                memberUser.ShippingAddressProvince = !string.IsNullOrEmpty(model.ShippingAddressProvince) ? model.ShippingAddressProvince.Trim() : string.Empty;
            }
            try
            {
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        private string formatPostalCode(string code, int secondIndex)
        {
            var result = code.Substring(0, 3) + " " + code.Substring(secondIndex);
            return result.ToUpper();
        }
        #endregion

        #region change and set password actions
        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            string passwordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$";
            Regex re = new Regex(passwordRegex);
            // Password validation
            if (!re.IsMatch(model.NewPassword))
            {
                ModelState.AddModelError("NewPassword", "The password must contain at least one uppercase, one lowercase, one digint and one special character.");
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        #endregion

        #region credit cards CRUD actions
        //
        // GET: /Manage/GetCreditCards
        [HttpGet]
        public JsonResult GetCreditCards()
        {
            string currentUserId = User.Identity.GetUserId();

            List<CreditCard> creditCards = new List<CreditCard>();
            using (CVGSAppEntities db = new CVGSAppEntities())
            {
                creditCards = db.CreditCards.Where(c => c.HolderId == currentUserId).ToList();
            }

            return Json(creditCards, JsonRequestBehavior.AllowGet);
        }

        //
        // GET: /Manage/GetCreditCard
        [HttpGet]
        public JsonResult GetCreditCard(int? Id)
        {
            if (Id == null)
            {
                return Json(new { Success = false, Error = "Information not received" }, JsonRequestBehavior.AllowGet);
            }

            CVGSAppEntities db = new CVGSAppEntities();
            try
            {
                var creditCard = db.CreditCards.Find(Id);

                if (creditCard == null)
                {
                    return Json(new { Success = false, Error = "Credit Card not found" }, JsonRequestBehavior.AllowGet);
                }

                return Json(creditCard, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { Success = false, Error = "Error occurred while retrieving credit card record" }, JsonRequestBehavior.AllowGet);
            }
        }

        //
        // POST: /Manage/AddCreditCard
        [HttpPost]
        public JsonResult EditCreditCard(CreditCard creditCard)
        {
            // Validation?

            // TODO: format credit card number before saving
            // TODO: format expiration date info?

            if (creditCard.Id == 0)
            {
                return Json(new { Success = false, Error = "Information not received" }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                // format credit card number
                string cardNumberFormatted = creditCard.CardNumber.Replace(" ", string.Empty);
                cardNumberFormatted = cardNumberFormatted.Length == 13 ? string.Format("{0:0000 0000 0000 0}", (long.Parse(cardNumberFormatted))) :
                    cardNumberFormatted.Length == 15 ? string.Format("{0:0000 0000 0000 000}", (long.Parse(cardNumberFormatted))) :
                        string.Format("{0:0000 0000 0000 0000}", (long.Parse(cardNumberFormatted)));
                //save the card in the database
                CVGSAppEntities db = new CVGSAppEntities();
                var cc = db.CreditCards.FirstOrDefault(c => c.Id == creditCard.Id);
                if (cc != null)
                {
                    cc.HolderId = User.Identity.GetUserId();
                    cc.CardNumber = cardNumberFormatted;
                    cc.ExpirationDate = creditCard.ExpirationDate;
                    db.SaveChanges();
                    return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Success = false, Error = "Credit card not found" }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { Success = false, Error = "Unexpected error occurred while updating credit card information" }, JsonRequestBehavior.AllowGet);
            }
        }

        //
        // POST: /Manage/AddCreditCard
        [HttpPost]
        public JsonResult AddCreditCard(CreditCard creditCard)
        {
            // Validation?

            // TODO: format credit card number before saving
            // TODO: format expiration date info?
            creditCard.HolderId = User.Identity.GetUserId();
            CVGSAppEntities db = new CVGSAppEntities();
            try
            {
                // format credit card number
                string cardNumberFormatted = creditCard.CardNumber.Replace(" ", string.Empty);
                cardNumberFormatted = cardNumberFormatted.Length == 13 ? string.Format("{0:0000 0000 0000 0}", (long.Parse(cardNumberFormatted))) :
                    cardNumberFormatted.Length == 15 ? string.Format("{0:0000 0000 0000 000}", (long.Parse(cardNumberFormatted))) :
                        string.Format("{0:0000 0000 0000 0000}", (long.Parse(cardNumberFormatted)));
                creditCard.CardNumber = cardNumberFormatted;

                //save the card in the database
                db.CreditCards.Add(creditCard);
                db.SaveChanges();
                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { Success = false, Error = "Error occurred while saving card information" }, JsonRequestBehavior.AllowGet);
            }
        }

        //
        // POST: /Manage/DeleteCreditCard
        [HttpPost]
        public JsonResult DeleteCreditCard(int? Id)
        {
            if (Id == null)
            {
                return Json(new { Success = false, Error = "Information not received" }, JsonRequestBehavior.AllowGet);
            }

            CVGSAppEntities db = new CVGSAppEntities();
            try
            {
                var creditCard = db.CreditCards.FirstOrDefault(c => c.Id == Id);

                if (creditCard == null)
                {
                    return Json(new { Success = false, Error = "Credit Card not found" }, JsonRequestBehavior.AllowGet);
                }

                db.CreditCards.Remove(creditCard);
                db.SaveChanges();

                return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { Success = false, Error = "Error occurred while deleting credit card record" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region actions generated by asp.net that we don't use
        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            SavedAccountDetailsSuccess,
            Error
        }

#endregion
    }
}