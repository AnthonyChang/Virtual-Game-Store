using CVGS.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace CVGS.Areas.Admin.Controllers
{
    public class EventController : Controller
    {
        const string ADMIN_ROLE = "Employee";

        // GET: Admin/Event
        [HttpGet]
        [CustomAuthorize(Roles = ADMIN_ROLE)]
        public ActionResult Index()
        {
            CVGSAppEntities db = new CVGSAppEntities();
            return View(db.Events.OrderBy(e => e.StartTime));
        }

        // GET: Admin/Event/Create
        [HttpGet]
        [CustomAuthorize(Roles = ADMIN_ROLE)]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/Event/Create
        [HttpPost]
        [CustomAuthorize(Roles = ADMIN_ROLE)]
        public async Task<ActionResult> Create(Event newEvent)
        {
            var db = new CVGSAppEntities();

            // Event name unique
            if (db.Events.Any(e => e.Title == newEvent.Title))
            {
               ModelState.AddModelError("Title", "An event with this name already exists.");
            }

            ValidateEvent(newEvent);

            if (!ModelState.IsValid)
            {
                return View(newEvent);
            }

            try
            {
                newEvent.Id = Guid.NewGuid().ToString();
                db.Events.Add(newEvent);
                await db.SaveChangesAsync();
                TempData["message"] = $"Record for '{newEvent.Title}' successfully added";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.GetBaseException().Message}");
                return View(newEvent);
            }
        }

        // GET: Admin/Event/Edit/{id}
        [HttpGet]
        [CustomAuthorize(Roles = ADMIN_ROLE)]
        public async Task<ActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(Index));
            }

            CVGSAppEntities db = new CVGSAppEntities();
            var model = await db.Events.FindAsync(id);
            if (model == null)
            {
                TempData["message"] = string.Format("Event record with {0} id was not found", id);
                RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: Admin/Event/Edit/{id}
        [HttpPost]
        [CustomAuthorize(Roles = ADMIN_ROLE)]
        public async Task<ActionResult> Edit(Event updatedEvent)
        {
            CVGSAppEntities db = new CVGSAppEntities();

            if (updatedEvent == null || string.IsNullOrEmpty(updatedEvent.Id))
            {
                return RedirectToAction(nameof(Index));
            }

            // Unqiue Name
            if(db.Events.Any(e => e.Title == updatedEvent.Title && e.Id != updatedEvent.Id))
            {
                ModelState.AddModelError("Title", "An event with this name already exists.");
            }

            try
            {
                var existingEvent = await db.Events.FindAsync(updatedEvent.Id);
                if (existingEvent == null)
                {
                    TempData["message"] = string.Format("Event record with {0} id was not found", updatedEvent.Id);
                    RedirectToAction(nameof(Index));
                }

                ValidateEvent(updatedEvent);

                if (!ModelState.IsValid)
                {
                    return View(updatedEvent);
                }

                existingEvent.Title = updatedEvent.Title;
                existingEvent.Description = updatedEvent.Description;
                existingEvent.StartTime = updatedEvent.StartTime;
                existingEvent.EndTime = updatedEvent.EndTime;
                existingEvent.MaxAttendeeNumber = updatedEvent.MaxAttendeeNumber;
                existingEvent.IsIRL = updatedEvent.IsIRL;
                existingEvent.Location = updatedEvent.Location;

                await db.SaveChangesAsync();
                TempData["message"] = $"Record for '{updatedEvent.Title}' successfully updated";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(updatedEvent);
            }
        }

        // GET: Admin/Event/Delete/{id}
        [HttpGet]
        [CustomAuthorize(Roles = ADMIN_ROLE)]
        public async Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(Index));
            }

            CVGSAppEntities db = new CVGSAppEntities();
            var model = await db.Events.FindAsync(id);
            if (model == null)
            {
                TempData["message"] = string.Format("Event record with {0} id was not found", id);
                RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // POST: Admin/Event/Delete/{id}
        [HttpPost]
        [CustomAuthorize(Roles = ADMIN_ROLE)]
        public async Task<ActionResult> Delete(Event model)
        {
            CVGSAppEntities db = new CVGSAppEntities();
            try
            {
                db.Events.Attach(model);
                db.Events.Remove(model);
                await db.SaveChangesAsync();
                TempData["message"] = $"Record successfully deleted";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.GetBaseException().Message}");
                return View(model);
            }
        }

        private void ValidateEvent(Event model)
        {
            // Start date must be in future
            if (model.StartTime < DateTime.Now)
            {
                ModelState.AddModelError("StartTime", "Event start time must be in future.");
            }

            // End date must be in future
            if (model.EndTime < DateTime.Now)
            {
                ModelState.AddModelError("EndTime", "Event end time must be in future.");
            }

            // End date must be after start time.
            if (model.EndTime < model.StartTime)
            {
                ModelState.AddModelError("EndTime", "Event can not end before start time.");
            }
        }
    }
}