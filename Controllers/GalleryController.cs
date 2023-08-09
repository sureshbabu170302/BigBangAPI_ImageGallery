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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            try
            {
                var galleryImage = await _context.Galleries.FindAsync(id);

                if (galleryImage == null)
                {
                    return NotFound();
                }

                // Delete the image from Blob Storage
                await DeleteImageFromBlobStorage(galleryImage.Gallery_Image);

                _context.Galleries.Remove(galleryImage);
                await _context.SaveChangesAsync();

                return Ok("Image deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error deleting Gallery image: {ex.Message}");
            }
        }

        private async Task DeleteImageFromBlobStorage(string imageUrl)
        {
            try
            {
                string connectionString = _blobOptions.ConnectionString;
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

                // Get the blob name from the imageUrl
                Uri uri = new Uri(imageUrl);
                string blobName = uri.Segments.Last();

                // Get the container name from the connection string (assuming it's the same for all images)
                string containerName = "images";

                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // Delete the blob from Blob Storage
                await containerClient.DeleteBlobIfExistsAsync(blobName);
            }
            catch (Exception ex)
            {
                // Handle any exception that occurs during image deletion.
                // You may choose to log the error or take other appropriate actions.
                throw new Exception($"Error deleting image from Blob Storage: {ex.Message}");
            }
        }
    }
}
