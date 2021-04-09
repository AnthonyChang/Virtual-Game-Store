using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CVGS.Models
{
    public class OrdersViewModel
    {
        public string Id { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MMMM dd, yyyy, h:mm tt}")]
        [Display(Name = "Order Placed")]
        public DateTime OrderPlaced { get; set; }

        [Display(Name = "Items")]
        public int ItemNumber { get; set; }

        [DataType(DataType.Currency)]
        [Display(Name = "Total")]
        public decimal Total { get; set; }

        public List<GameThumbView> Games { get; set; }
    }

    public class GameThumbView
    {
        public string Id { get; set; }
        public string ImageUrl { get; set; }
    }
}