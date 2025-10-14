using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class ManpowerController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ManpowerController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var vm = new ManPowerViewModel
            {
                ManPower = new ManPower(),
                ManPowerList = _context.ManPowers
                                       .Include(mp => mp.Service) // Include Service for display
                                       .ToList(),
                Services = _context.Services
                                   .Where(s => s.IsActive)
                                   .Select(s => new SelectListItem
                                   {
                                       Text = s.Name,
                                       Value = s.Id.ToString()
                                   })
                                   .ToList()
            };

            return View(vm);
        }

        // POST: Save
        [HttpPost]
        public IActionResult Save(ManPowerViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Reload ManPower list and Services if validation fails
                vm.ManPowerList = _context.ManPowers
                                          .Include(mp => mp.Service)
                                          .ToList();
                vm.Services = _context.Services
                                      .Where(s => s.IsActive)
                                      .Select(s => new SelectListItem
                                      {
                                          Text = s.Name,
                                          Value = s.Id.ToString()
                                      })
                                      .ToList();
                return View("Index", vm);
            }

            if (vm.ManPower.Id == 0)
            {
                _context.ManPowers.Add(vm.ManPower);
            }
            else
            {
                _context.ManPowers.Update(vm.ManPower);
            }

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: Edit
        public IActionResult Edit(int id)
        {
            var manPower = _context.ManPowers.Find(id);
            if (manPower == null) return NotFound();

            var vm = new ManPowerViewModel
            {
                ManPower = manPower,
                ManPowerList = _context.ManPowers
                                       .Include(mp => mp.Service)
                                       .ToList(),
                Services = _context.Services
                                   .Where(s => s.IsActive)
                                   .Select(s => new SelectListItem
                                   {
                                       Text = s.Name,
                                       Value = s.Id.ToString()
                                   })
                                   .ToList()
            };

            return View("Index", vm);
        }

        // GET: Delete
        public IActionResult Delete(int id)
        {
            var manPower = _context.ManPowers.Find(id);
            if (manPower != null)
            {
                _context.ManPowers.Remove(manPower);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
