using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize(Roles = "Manager,Admin,Secretary")]
    public class RoleManagementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoleManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Role Management
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Roles
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync();
            return View(roles);
        }

        // GET: Create Role
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create Role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role role)
        {
            if (ModelState.IsValid)
            {
                role.CreatedAt = DateTime.UtcNow;
                role.IsActive = true;
                
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Role '{role.Name}' created successfully!";
                return RedirectToAction("Index");
            }
            return View(role);
        }

        // GET: Edit Role
        public async Task<IActionResult> Edit(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }
            return View(role);
        }

        // POST: Edit Role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Role role)
        {
            if (id != role.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    role.UpdatedAt = DateTime.UtcNow;
                    _context.Update(role);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Role '{role.Name}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RoleExists(role.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(role);
        }

        // POST: Delete Role
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
            {
                TempData["Error"] = "Role not found.";
                return RedirectToAction("Index");
            }

            // Check if any employees are using this role
            var employeesUsingRole = await _context.Employees
                .AnyAsync(e => e.RoleId == id);

            if (employeesUsingRole)
            {
                TempData["Error"] = $"Cannot delete role '{role.Name}' because employees are assigned to it.";
                return RedirectToAction("Index");
            }

            role.IsActive = false;
            role.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            TempData["Success"] = $"Role '{role.Name}' deactivated successfully!";
            return RedirectToAction("Index");
        }

        private bool RoleExists(int id)
        {
            return _context.Roles.Any(e => e.Id == id);
        }
    }
}
