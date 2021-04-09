using CVGS.Data;
using System;
using System.ComponentModel.DataAnnotations;

namespace CVGS.Areas.Admin.Models
{
    public class GameReviewApprovalModel
    {
        public string Id { get; set; }
        [Display(Name = "Game Title")]
        public string GameName { get; set; }
        [Display(Name = "Posted By")]
        public string CreatedBy { get; set; }
        [Display(Name = "Content")]
        public string ReviewText { get; set; }
        [Display(Name = "Poster On")]
        public DateTime CreatedOn { get; set; }
        public DateTime EditedOn { get; set; }
        public bool Approved { get; set; }
    }

    public static class GameReviewApprovalModelExtensions
    {
        public static GameReviewApprovalModel GameReviewToGameReviewApprovalModel(this GameReview review, string reviewerName, string gameTitle)
        {
            return new GameReviewApprovalModel
            {
                Id = review.Id,
                CreatedBy = reviewerName,
                GameName = gameTitle,
                ReviewText = review.ReviewText,
                CreatedOn = review.CreatedOn,
                EditedOn = review.EditedOn,
                Approved = review.Approved
            };
        }
    }
}