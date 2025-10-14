using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class ClientsServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        public ClientsServicesController(ApplicationDbContext applicationDbContext, IWebHostEnvironment env)
        {
            _context = applicationDbContext;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var services = await _context.ClientsServices
               .OrderByDescending(x => x.Id)
               .ToListAsync();
            return View(services);
        }
        [HttpGet]
        public IActionResult AddClientService()
        {
            return View(new ClientsServices());
        }

        public IActionResult ClientsServicesList()
        {
            var services = _context.ClientsServices.ToList();
            return View(services);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveClientService(ClientsServices model)
        {
            if (!ModelState.IsValid)
                return View("AddClientService", model);

            // Call private method to handle image upload
            string imagePath = await SaveImageAsync(model.ImageFile, model.ExistingImageUrl);

            if (model.Id == 0)
            {
                // Create new record
                _context.ClientsServices.Add(new ClientsServices
                {
                    Title = model.Title,
                    Description = model.Description,
                    ExistingImageUrl = imagePath,
                    IsApproved = model.IsApproved
                });
            }
            else
            {
                // Update existing record
                var entity = await _context.ClientsServices.FindAsync(model.Id);
                if (entity == null) return NotFound();

                entity.Title = model.Title;
                entity.Description = model.Description;
                entity.ExistingImageUrl = imagePath;
                entity.IsApproved = model.IsApproved;

                _context.ClientsServices.Update(entity);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClientService(int id)
        {
            var service = await _context.ClientsServices.FindAsync(id);
            if (service == null) return NotFound();

            // Delete image from wwwroot
            DeleteImageFile(service.ExistingImageUrl);

            _context.ClientsServices.Remove(service);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        private void DeleteImageFile(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var filePath = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/').Replace("/", "\\"));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditClientService(int id)
        {
            var service = await _context.ClientsServices.FindAsync(id);
            if (service == null) return NotFound();

            return View("AddClientService", service); // reuse same form
        }

        private async Task<string> SaveImageAsync(IFormFile? imageFile, string? existingImageUrl)
        {
            if (imageFile == null || imageFile.Length == 0)
                return existingImageUrl ?? string.Empty; // Keep existing if no new file uploaded

            var uploadsFolder = Path.Combine(_env.WebRootPath, "photos");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return "/photos/" + fileName;
        }


    }
}