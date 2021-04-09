using CVGS.Areas.Admin.Models;
using CVGS.Data;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using System.Web;

namespace CVGS.Areas.Admin.Controllers
{
    public class ReviewController : Controller
    {
        private readonly CVGSAppEntities db;

        public ReviewController()
        {
            if (db == null)
            {
                db = new CVGSAppEntities();
            }
        }

        const string ADMIN_ROLE = "Employee";
        // GET: Admin/Review
        [HttpGet]
        [CustomAuthorize(Roles = ADMIN_ROLE)]
        public ActionResult Index()
        {
            var reviews = db.GameReviews.Where(r => !r.Approved);
            var model = new List<GameReviewApprovalModel>();
            var applicationUserManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            foreach (var review in reviews)
            {
                var gameTitle = db.Games.FirstOrDefault(g => g.Id == review.GameId).Title;
                var reviewer = applicationUserManager.FindById(review.CreatedBy).UserName;
                model.Add(review.GameReviewToGameReviewApprovalModel(reviewer, gameTitle));
            }
            return View(model);
        }

       //[HttpPost]
        [CustomAuthorize(Roles = ADMIN_ROLE)]
        public async Task<ActionResult> ApproveReview(string id, bool approved)
        {
            if (!string.IsNullOrEmpty(id))
            {
                try
                {
                    var model = await db.GameReviews.FindAsync(id);
                    if (model == null)
                    {
                        TempData["message"] = string.Format("Review with {0} id was not found", id);
                    }
                    else
                    {
                        if (approved)
                        {
                            model.Approved = true;
                            await db.SaveChangesAsync();
                        }
                        else
                        {
                            db.GameReviews.Remove(model);
                            await db.SaveChangesAsync();
                        }
                    }
                    RedirectToAction(nameof(Index));
                }
                catch
                {

                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}