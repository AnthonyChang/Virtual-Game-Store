using CVGS.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CVGS.Models
{
    public class EventDetailsViewModel
    {
        public Event Event { get; set; }

        [Display(Name = "Event Type")]
        public string EventType { get; set; }

        public bool UserRegistered { get; set; }
    }

}