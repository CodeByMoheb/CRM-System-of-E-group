using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize(Roles = "Manager,Admin,Secretary")] 
    public class MemberDirectoryController : Controller
    {
        private readonly ApplicationDbContext _db;

        public MemberDirectoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Buyers CRUD (JSON)
        [HttpGet("/MemberDirectory/BuyersJson")]
        [Produces("application/json")]
        public async Task<IActionResult> Buyers()
        {
            var list = await _db.BuyerContacts.OrderByDescending(x => x.CreatedAt).ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> SaveBuyer([FromBody] BuyerContact model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var exists = await _db.BuyerContacts.AsTracking().FirstOrDefaultAsync(x => x.Id == model.Id);
                if (exists == null)
                {
                    model.Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id;
                    model.CreatedAt = DateTime.UtcNow;
                    _db.BuyerContacts.Add(model);
                }
                else
                {
                    _db.Entry(exists).CurrentValues.SetValues(model);
                    _db.Entry(exists).State = EntityState.Modified;
                }
                await _db.SaveChangesAsync();
                return Json(model);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBuyer(Guid id)
        {
            var entity = await _db.BuyerContacts.FindAsync(id);
            if (entity == null) return NotFound();
            _db.BuyerContacts.Remove(entity);
            await _db.SaveChangesAsync();
            return Ok();
        }

        // Clients CRUD (JSON)
        [HttpGet("/MemberDirectory/ClientsJson")]
        [Produces("application/json")]
        public async Task<IActionResult> Clients()
        {
            var list = await _db.ClientContacts.OrderByDescending(x => x.CreatedAt).ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> SaveClient([FromBody] ClientContact model)
        {
            try
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var exists = await _db.ClientContacts.AsTracking().FirstOrDefaultAsync(x => x.Id == model.Id);
                if (exists == null)
                {
                    model.Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id;
                    model.CreatedAt = DateTime.UtcNow;
                    _db.ClientContacts.Add(model);
                }
                else
                {
                    _db.Entry(exists).CurrentValues.SetValues(model);
                    _db.Entry(exists).State = EntityState.Modified;
                }
                await _db.SaveChangesAsync();
                return Json(model);
            }
            catch (Exception ex)
            {
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            var entity = await _db.ClientContacts.FindAsync(id);
            if (entity == null) return NotFound();
            _db.ClientContacts.Remove(entity);
            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}


