using ImageGallery.Models;
using Microsoft.EntityFrameworkCore;

namespace ImageGallery.Data
{
    public class ImageGalleryContext : DbContext
    {
        public DbSet<Gallery> Galleries { get; set; }

        public ImageGalleryContext(DbContextOptions<ImageGalleryContext> options) : base(options)
        {
        }

    }
}
