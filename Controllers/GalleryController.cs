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
            _blobOptions = blobOptions.Value; // Initialize the field with the BlobOptions value
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
                    return Forbid(); // Return a forbidden response if Admin_Id is not 1
                }

                if (hotelViewModel.Image == null || hotelViewModel.Image.Length == 0)
                {
                    return BadRequest("No image file was provided");
                }

                // Upload the image to Azure Blob Storage
                string imageUrl = await UploadImageToBlobStorage(hotelViewModel.Image);

                // Save the gallery image details to the database
                Gallery galleryImages = new Gallery
                {
                    Admin_Id = hotelViewModel.Admin_Id,
                    Gallery_Image = imageUrl // Save the image URL in the database
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
            // Generate a unique blob name based on the uploaded image
            string blobName = $"{Guid.NewGuid().ToString()}{Path.GetExtension(imageFile.FileName)}";

            // Retrieve the connection string and container name from BlobOptions
            string connectionString = _blobOptions.ConnectionString;
            string containerName = "images";

            // Create the BlobServiceClient
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Create the container if it does not exist
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            // Get a reference to the blob
            BlobClient blobClient = containerClient.GetBlobClient(blobName);

            // Upload the image to the blob
            using (var stream = imageFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, true);
            }

            // Return the URL of the uploaded image
            return blobClient.Uri.ToString();
        }
    }
}
