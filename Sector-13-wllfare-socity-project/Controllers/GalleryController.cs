using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using Sector_13_Welfare_Society___Digital_Management_System.Services;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize(Roles = "Manager")]
    public class GalleryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;

        public GalleryController(ApplicationDbContext context, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
        }

        // GET: /Gallery/Manage
        public async Task<IActionResult> Manage()
        {
            // Fetch gallery images with their categories if needed
            
            var viewModel = new GalleryViewModel
            {
                Gallery = new GalleryImageVm(),
                Galleries = await _context.GalleryImages
                    .Include(g => g.Category) // make sure GalleryImage has navigation property Category
                    .OrderByDescending(g => g.CreatedAt)
                    .ToListAsync()
            };
            return View(viewModel);
        }


        // GET: /Gallery/Upload



        public IActionResult Upload()
        {
            var viewModel = new GalleryViewModel
            {
                Categories = _context.Categories
                    .Where(c => c.IsActive)                 // only active categories
                    .OrderBy(c => c.Serial)                 // ordered by Serial
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGallery(GalleryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories
                    .Where(c => c.Type == "Gallery")
                    .OrderBy(c => c.Serial)
                    .ToListAsync();

                return View("Upload", model);
            }

            var galleryVm = model.Gallery;

            string imageUrl = galleryVm.ExistingImageUrl ?? string.Empty;

            // Handle file upload - via Cloudinary service
            string? newPublicId = null;
            if (galleryVm.ImageFile != null && galleryVm.ImageFile.Length > 0)
            {
                var uploadResult = await _cloudinaryService.UploadImageAsync(galleryVm.ImageFile, "gallery");
                if (!uploadResult.success)
                {
                    TempData["Error"] = uploadResult.error ?? "Cloud upload failed.";
                    return RedirectToAction("Upload", "Gallery");
                }
                imageUrl = uploadResult.url!;
                newPublicId = uploadResult.publicId;
            }

            try
            {
                var userName = User.Identity?.Name ?? "System";

                if (galleryVm.Id == 0) // Add
                {
                    var gallery = new GalleryImage
                    {
                        Title = galleryVm.Title,
                        CategoryId = galleryVm.CategoryId,
                        ImageUrl = imageUrl,
                        PublicId = newPublicId,
                        CreatedAt = DateTime.Now,
                        CreatedBy = userName,
                        ApprovedAt = DateTime.Now,
                        ApprovedBy = userName
                    };

                    await _context.GalleryImages.AddAsync(gallery);
                    TempData["Success"] = "Gallery image added successfully!";
                }
                else // Update
                {
                    var existingGallery = await _context.GalleryImages.FindAsync(galleryVm.Id);
                    if (existingGallery == null)
                    {
                        TempData["Error"] = "Gallery image not found!";
                        return RedirectToAction(nameof(Upload));
                    }

                    existingGallery.Title = galleryVm.Title;
                    existingGallery.CategoryId = galleryVm.CategoryId;
                    // If a new image was uploaded, replace URL and publicId, and delete old asset
                    if (!string.IsNullOrEmpty(newPublicId) && !string.IsNullOrEmpty(imageUrl))
                    {
                        var oldPublicId = existingGallery.PublicId;
                        existingGallery.ImageUrl = imageUrl;
                        existingGallery.PublicId = newPublicId;
                        if (!string.IsNullOrWhiteSpace(oldPublicId))
                        {
                            _ = _cloudinaryService.DeleteImageAsync(oldPublicId);
                        }
                    }
                    existingGallery.ApprovedAt = DateTime.Now;
                    existingGallery.ApprovedBy = userName;

                    TempData["Success"] = "Gallery image updated successfully!";
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while saving the image: {ex.Message}";
            }

            return RedirectToAction(nameof(Upload));
        }


        public IActionResult EditGallery(int id)
        {
            var gallery = _context.GalleryImages.Find(id);
            if (gallery == null) return NotFound();

            var galleryVm = new GalleryImageVm
            {
                Id = gallery.Id,
                Title = gallery.Title,
                CategoryId = gallery.CategoryId,
                ExistingImageUrl = gallery.ImageUrl,
                PublicId = gallery.PublicId
            };

            var viewModel = new GalleryViewModel
            {
                Gallery = galleryVm,
                Galleries = _context.GalleryImages.ToList(),
                Categories = _context.Categories.OrderBy(c => c.Serial).ToList()
            };

            return View("Upload", viewModel);
        }


        public IActionResult DeleteGallery(int id)
        {
            var gallery = _context.GalleryImages.Find(id);
            if (gallery != null)
            {
                // Delete from Cloudinary if we have a public id
                if (!string.IsNullOrWhiteSpace(gallery.PublicId))
                {
                    _ = _cloudinaryService.DeleteImageAsync(gallery.PublicId);
                }

                _context.GalleryImages.Remove(gallery);
                _context.SaveChanges();
            }

            return RedirectToAction("Manage"); // reload gallery page
        }

        // Test endpoints and direct upload helpers removed in favor of ICloudinaryService

        // POST: /Gallery/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.GalleryImages.FirstOrDefaultAsync(g => g.Id == id);
            if (item == null)
            {
                TempData["Error"] = "Image not found.";
                return RedirectToAction(nameof(Manage));
            }

            if (!string.IsNullOrWhiteSpace(item.PublicId))
            {
                await _cloudinaryService.DeleteImageAsync(item.PublicId);
            }

            _context.GalleryImages.Remove(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Image removed.";
            return RedirectToAction(nameof(Manage));
        }

        
    }
}