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
using CVGS.Models;
using Microsoft.AspNet.Identity;

namespace CVGS.Controllers
{
    public class EventsController : Controller
    {
        private CVGSAppEntities db = new CVGSAppEntities();

        // GET: Events
        public ActionResult Index()
        {
            CVGSAppEntities db = new CVGSAppEntities();
            string userId = User.Identity.GetUserId();
            List<EventDetailsViewModel> model = new List<EventDetailsViewModel> { };

            IQueryable<Event> events = db.Events.Where(item => item.EndTime > DateTime.Now);

            foreach (Event @event in events)
            {
                EventDetailsViewModel modelObject = new EventDetailsViewModel
                {
                    Event = @event,
                    EventType = @event.IsIRL ? "In Person" : "Online",
                    UserRegistered = db.EventAttendees.Any(u => u.EventId == @event.Id && u.AttendeeId == userId)
                };

                model.Add(modelObject);
            }

            return View(model);
        }

        // GET: Events/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Event @event = await db.Events.FindAsync(id);
            if (@event == null)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId();
            var model = new EventDetailsViewModel();
            model.Event = @event;

            // Get user registration status
            model.UserRegistered = db.EventAttendees.Any(u => u.EventId == @event.Id && u.AttendeeId == userId);

            return View(model);
        }

        // Get: Events/Register
        public async Task<ActionResult> Register(string eventId)
        {
            var userId = User.Identity.GetUserId();
            bool validRegister = true;

            // Check if event exists
            if (!eventExists(eventId))
            {
                TempData["message"] = "Event does not exist.";
                validRegister = false;
            }

            // Check if user is not registered
            if (db.EventAttendees.Any(i => i.EventId == eventId && i.AttendeeId == userId))
            {
                TempData["message"] = "You have already registered for this event.";
                validRegister = false;
            }

            if (validRegister)
            {
                db.EventAttendees.Add(new EventAttendee { EventId = eventId, AttendeeId = userId });
                var eventToUpdate = db.Events.Find(eventId);
                eventToUpdate.AttendeeNumber += 1;
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Details", new RouteValueDictionary(new { controller = "Events", action = "Details", Id = eventId }));
        }

        // Get: Events/Unregister
        public async Task<ActionResult> Unregister(string eventId)
        {
            var userId = User.Identity.GetUserId();
            var eventAttendee = db.EventAttendees.FirstOrDefault(e => e.EventId == eventId && e.AttendeeId == userId);
            bool validRegister = true;

            // Check if event exists
            if (!eventExists(eventId))
            {
                TempData["message"] = "Event does not exist.";
                validRegister = false;
            }

            // Check if user is registered
            if (eventAttendee == null)
            {
                TempData["message"] = "You are not registered for this event.";
                validRegister = false;
            }

            if (validRegister)
            {
                db.EventAttendees.Remove(eventAttendee);
                var eventToUpdate = db.Events.Find(eventId);
                eventToUpdate.AttendeeNumber -= 1;
                await db.SaveChangesAsync();
            }

            return RedirectToAction("Details", new RouteValueDictionary(new { controller = "Events", action = "Details", Id = eventId }));
        }

        private bool eventExists(string Id)
        {
            return db.Events.Any(c => c.Id == Id);
        }
    }
}
