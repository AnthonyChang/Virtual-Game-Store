using System;
using System.ComponentModel.DataAnnotations;

namespace CVGS.Data
{
    [MetadataType(typeof(GameMetaData))]
    public partial class Game
    {
        public string CategoryName { get; set; }
        public string PlatformName { get; set; }
    }
    public class GameMetaData
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Title")]
        public string Title;

        [Display(Name = "Category")]
        public Nullable<int> CategoryId;

        [Display(Name = "Platform")]
        public Nullable<int> PlatformId;

        [Required]
        [Display(Name = "Description")]
        public string Description;

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Price")]
        public decimal Price;

        [Display(Name = "Release Year")]
        public Nullable<int> ReleaseYear;

        [Required]
        [Display(Name = "Image URL")]
        public string ImageUrl;
    }
}