using ImageGallery.Data;
using ImageGallery.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;

namespace ImageGallery.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GalleryController : ControllerBase
    {
        private readonly ImageGalleryContext _context;
        private readonly BlobOptions _blobOptions;

        public GalleryController(ImageGalleryContext context, IOptions<BlobOptions> blobOptions)
        {
            _context = context;
            _blobOptions = blobOptions.Value;
        }

        [HttpGet]
        public async Task<IActionResult> GetImages()
        {
            try
            {
                var images = await _context.Galleries.ToListAsync();
                return Ok(images);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error retrieving Images: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddImages([FromForm] HotelCreateViewModel hotelViewModel)
        {
            try
            {
                if (hotelViewModel.Admin_Id != 1)
                {
                    return Forbid(); 
                }

                if (hotelViewModel.Image == null || hotelViewModel.Image.Length == 0)
                {
                    return BadRequest("No image file was provided");
                }


                string imageUrl = await UploadImageToBlobStorage(hotelViewModel.Image);

                Gallery galleryImages = new Gallery
                {
                    Admin_Id = hotelViewModel.Admin_Id,
                    Gallery_Image = imageUrl 
                };

                _context.Galleries.Add(galleryImages);
                await _context.SaveChangesAsync();

                return Ok(galleryImages.Gallery_Image);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error uploading Gallery image: {ex.Message}");
            }
        }

        private async Task<string> UploadImageToBlobStorage(IFormFile imageFile)
        {
            string blobName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(imageFile.FileName)}";

            string connectionString = _blobOptions.ConnectionString;
            string containerName = "images";

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            using (var stream = imageFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            return blobClient.Uri.ToString();
        }
    }
}
