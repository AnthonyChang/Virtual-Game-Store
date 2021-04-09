using CVGS.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CVGS.Models
{
    public class GameViewModel : Game
    {
        public bool IsInUserCart { get; set; }
        public bool IsPurchased { get; set; }
        public bool IsInWishList { get; set; }
        public ReviewListModel Reviews { get; set; }

        public GameViewModel(Game game)
        {
            this.Id = game.Id;
            this.Title = game.Title;
            this.CategoryId = game.CategoryId;
            this.CategoryName = game.CategoryName;
            this.PlatformId = game.PlatformId;
            this.PlatformName = game.PlatformName;
            this.Description = game.Description;
            this.Price = game.Price;
            this.ReleaseYear = game.ReleaseYear;
            this.ImageUrl = game.ImageUrl;
            this.Rating = game.Rating;
        }
    }

    public class ReviewListModel
    {
        public string GameId { get; set; }
        public List<ReviewViewModel> ApprovedReviews { get; set; }
        public List<ReviewViewModel> UnapprovedReviewsByUser { get; set; }
    }

    public class ReviewViewModel
    {
        public string Id { get; set; }
        public string ReviewerId { get; set; }
        public string Reviewer { get; set; }
        public string ReviewText { get; set; }
        public DateTime EditedOn { get; set; }
    }
}