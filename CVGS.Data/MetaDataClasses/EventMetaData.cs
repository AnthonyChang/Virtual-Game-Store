using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVGS.Data
{
    [MetadataType(typeof(EventMetaData))]
    public partial class Event
    {

    }

    public class EventMetaData
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title;

        [Required]
        [Display(Name = "Description")]
        public string Description;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "Start Time")]
        public DateTime StartTime;

        [Required]
        [DataType(DataType.DateTime)]
        [Display(Name = "End Time")]
        public DateTime EndTime;

        [Required]
        [Display(Name = "Maximum Attendees")]
        public int MaxAttendeeNumber;

        [Display(Name = "Current Attendees")]
        public int AttendeeNumber;

        [Display(Name = "Physical Location")]
        public bool IsIRL;

        [Required]
        [Display(Name = "Location")]
        public string Location;
    }
}
