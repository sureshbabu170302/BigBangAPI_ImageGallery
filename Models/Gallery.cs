using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ImageGallery.Models
{
    public class Gallery
    {
        [Key]
        public int Image_Id { get; set; }
        public int Admin_Id { get; set; }
        public string Gallery_Image { get; set; }
    }

    public class HotelCreateViewModel
    {
        public int Admin_Id { get; set; }
        public IFormFile Image { get; set; }
    }
}