using Microsoft.AspNetCore.Mvc;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class LocationChargeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public LocationChargeController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult LocationCharge()
        {
            var vm = new LocationChargeViewModel
            {
                LocationCharge = new LocationCharge(),
                LocationChargeList = _context.LocationCharges.ToList()
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult Save(LocationChargeViewModel vm)
        {
            var loggedInEmail = User.Identity?.Name;
            if (vm.LocationCharge.Id == 0)
            {
                vm.LocationCharge.CreatedAt = DateTime.Now;
                vm.LocationCharge.CreatedBy = loggedInEmail;
                _context.LocationCharges.Add(vm.LocationCharge);
            }
            else
            {
                vm.LocationCharge.ApprovedAt = DateTime.Now;
                vm.LocationCharge.ApprovedBy = loggedInEmail;
                _context.LocationCharges.Update(vm.LocationCharge);
            }

            _context.SaveChanges();
            return RedirectToAction("LocationCharge");
        }

        public IActionResult Edit(int id)
        {
            var charge = _context.LocationCharges.Find(id);
            if (charge == null) return NotFound();

            var vm = new LocationChargeViewModel
            {
                LocationCharge = charge,
                LocationChargeList = _context.LocationCharges.ToList()
            };

            return View("LocationCharge", vm);
        }

        public IActionResult Delete(int id)
        {
            var charge = _context.LocationCharges.Find(id);
            if (charge != null)
            {
                _context.LocationCharges.Remove(charge);
                _context.SaveChanges();
            }

            return RedirectToAction("LocationCharge");
        }
    }
}
