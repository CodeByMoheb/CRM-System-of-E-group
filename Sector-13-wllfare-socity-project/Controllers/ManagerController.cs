using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize(Roles = "Manager,Admin")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Questionnaires()
        {
            var questions = await _context.AuditQuestions
                .Include(q => q.Service)
                .OrderBy(q => q.Category)
                .ThenBy(q => q.SortOrder)
                .ToListAsync();

            var services = await _context.Services.Where(s => s.IsActive).ToListAsync();
            var categories = await _context.AuditQuestions
                .Select(q => q.Category)
                .Distinct()
                .ToListAsync();

            var viewModel = new ManagerQuestionnaireViewModel
            {
                Questions = questions,
                Services = services,
                Categories = categories,
                TotalQuestions = questions.Count,
                ActiveQuestions = questions.Count(q => q.IsActive),
                QuestionsByCategory = questions.GroupBy(q => q.Category)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuestion(AuditQuestion question)
        {
            if (ModelState.IsValid)
            {
                question.CreatedAt = DateTime.Now;
                question.UpdatedAt = DateTime.Now;
                question.IsActive = true;
                
                // Set sort order to next available number in category
                var maxSort = await _context.AuditQuestions
                    .Where(q => q.Category == question.Category)
                    .MaxAsync(q => (int?)q.SortOrder) ?? 0;
                question.SortOrder = maxSort + 1;

                _context.AuditQuestions.Add(question);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Question created successfully!";
            }
            else
            {
                TempData["Error"] = "Please fill all required fields.";
            }

            return RedirectToAction("Questionnaires");
        }

        [HttpPost]
        public async Task<IActionResult> CreateBulkQuestions(ManagerQuestionnaireViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.BulkQuestions) || 
                string.IsNullOrWhiteSpace(model.SelectedCategory))
            {
                TempData["Error"] = "Please provide questions and select a category.";
                return RedirectToAction("Questionnaires");
            }

            var lines = model.BulkQuestions
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            var maxSort = await _context.AuditQuestions
                .Where(q => q.Category == model.SelectedCategory)
                .MaxAsync(q => (int?)q.SortOrder) ?? 0;

            foreach (var (line, index) in lines.Select((line, index) => (line, index)))
            {
                var question = new AuditQuestion
                {
                    QuestionText = line,
                    Category = model.SelectedCategory,
                    ServiceId = model.SelectedServiceId,
                    SortOrder = maxSort + index + 1,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.AuditQuestions.Add(question);
            }

            await _context.SaveChangesAsync();
            
            TempData["Success"] = $"{lines.Count} questions created successfully!";
            return RedirectToAction("Questionnaires");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleQuestionStatus(int id)
        {
            var question = await _context.AuditQuestions.FindAsync(id);
            if (question == null)
                return NotFound();

            question.IsActive = !question.IsActive;
            question.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isActive = question.IsActive });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuestionOrder(int[] questionIds)
        {
            for (int i = 0; i < questionIds.Length; i++)
            {
                var question = await _context.AuditQuestions.FindAsync(questionIds[i]);
                if (question != null)
                {
                    question.SortOrder = i + 1;
                    question.UpdatedAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> EditQuestion(int id)
        {
            var question = await _context.AuditQuestions
                .Include(q => q.Service)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
                return NotFound();

            ViewBag.Services = await _context.Services.Where(s => s.IsActive).ToListAsync();
            ViewBag.Categories = await _context.AuditQuestions
                .Select(q => q.Category)
                .Distinct()
                .ToListAsync();

            return View(question);
        }

        [HttpPost]
        public async Task<IActionResult> EditQuestion(AuditQuestion model)
        {
            if (ModelState.IsValid)
            {
                var question = await _context.AuditQuestions.FindAsync(model.Id);
                if (question == null)
                    return NotFound();

                question.QuestionText = model.QuestionText;
                question.Category = model.Category;
                question.ServiceId = model.ServiceId;
                question.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                TempData["Success"] = "Question updated successfully!";
                return RedirectToAction("Questionnaires");
            }

            ViewBag.Services = await _context.Services.Where(s => s.IsActive).ToListAsync();
            ViewBag.Categories = await _context.AuditQuestions
                .Select(q => q.Category)
                .Distinct()
                .ToListAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var question = await _context.AuditQuestions.FindAsync(id);
            if (question == null)
                return NotFound();

            _context.AuditQuestions.Remove(question);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> AuditReports()
        {
            var completedAudits = await _context.AuditSessions
                .Include(a => a.Booking)
                .ThenInclude(b => b.Service)
                .Include(a => a.Booking)
                .ThenInclude(b => b.Customer)
                .Include(a => a.Auditor)
                .Where(a => a.Status == "Completed")
                .OrderByDescending(a => a.AuditCompletedDate)
                .ToListAsync();

            return View(completedAudits);
        }

        [HttpGet]
        public async Task<IActionResult> CAPReports()
        {
            var capSessions = await _context.AuditSessions
                .Include(a => a.Booking)
                .ThenInclude(b => b.Service)
                .Include(a => a.Booking)
                .ThenInclude(b => b.Customer)
                .Include(a => a.CorrectiveActionPlans)
                .Where(a => a.RequiresCAP)
                .OrderByDescending(a => a.AuditCompletedDate)
                .ToListAsync();

            return View(capSessions);
        }
    }
}