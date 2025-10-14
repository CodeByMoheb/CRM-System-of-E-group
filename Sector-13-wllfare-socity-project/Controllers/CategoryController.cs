using iText.Kernel.Pdf.Canvas.Parser.ClipperLib;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System.Linq;
using System.Security.Claims;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Category()
        {
            var viewModel = new CategoryViewModel
            {
                Categories = _context.Categories.ToList(),
                Category = new CategoryVm()
            };
            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveCategory(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var categoryVm = model.Category;
            Category category;

            if (categoryVm.Id == 0) // Add
            {
                // Get next serial (if no category exists, start with 1)
                var nextSerial = _context.Categories.Any()
                    ? _context.Categories.Max(c => c.Serial) + 1
                    : 1;

                category = new Category
                {
                    Type = categoryVm.Type,
                    Value = categoryVm.Value,
                    Name = categoryVm.Name,
                    IsActive = categoryVm.IsActive,
                    Serial = nextSerial, // Auto assigned
                    CreatedAt = DateTime.Now,
                    CreatedBy = User.Identity?.Name ?? "System",
                    ApprovedAt = DateTime.Now,
                    ApprovedBy = User.Identity?.Name ?? "System"
                };

                _context.Categories.Add(category);
            }
            else // Update
            {
                category = _context.Categories.Find(categoryVm.Id);
                if (category == null) return NotFound();

                category.Type = categoryVm.Type;
                category.Value = categoryVm.Value;
                category.Name = categoryVm.Name;
                category.IsActive = categoryVm.IsActive;
                
                category.ApprovedAt = DateTime.Now;
                category.ApprovedBy = User.Identity?.Name ?? "System";

                _context.Categories.Update(category);
            }

            _context.SaveChanges();

            var categories = _context.Categories
                .OrderBy(c => c.Serial)
                .ToList();

            return PartialView("_CategoryList", categories);
        }




        public IActionResult EditCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null) return NotFound();

            var categoryVm = new CategoryVm
            {
                Id = category.Id,
                Type = category.Type,
                Value = category.Value,
                Name = category.Name,
                IsActive = category.IsActive,
                Serial = category.Serial,
                CreatedAt = category.CreatedAt,
                CreatedBy = category.CreatedBy,
                ApprovedAt = category.ApprovedAt,
                ApprovedBy = category.ApprovedBy
            };

            var viewModel = new CategoryViewModel
            {
                Category = categoryVm,
                Categories = _context.Categories.ToList()
            };
            return View("Category", viewModel);
        }

        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }
            return RedirectToAction("Category");
        }


    }
}
