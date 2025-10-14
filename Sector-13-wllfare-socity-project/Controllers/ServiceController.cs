using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize(Roles = "Manager,Secretary,Admin")]
    public class ServiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Service
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .Include(s => s.ServiceCategory)
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Name)
                .ToListAsync();
            
            return View(services);
        }

        
        // GET: Service/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Service/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service service)
        {
            if (ModelState.IsValid)
            {
                if (service.Id == 0)
                {
                    // Create - explicitly set ServiceCategoryId to null since we don't use categories
                    service.ServiceCategoryId = null;
                    service.CreatedAt = DateTime.Now;
                    service.UpdatedAt = DateTime.Now;
                    _context.Add(service);
                    TempData["Success"] = "Service created successfully.";
                }
                else
                {
                    // Edit - also ensure ServiceCategoryId is null
                    service.ServiceCategoryId = null;
                    service.UpdatedAt = DateTime.Now;
                    _context.Update(service);
                    TempData["Success"] = "Service updated successfully.";
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(service);
        }
        // GET: Service/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var service = await _context.Services
                .FirstOrDefaultAsync(m => m.Id == id);

            if (service == null)
                return NotFound();

            return PartialView("_ServiceDetailsPartial", service);
        }

        // GET: Service/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            // Explicitly use Create view for editing
            return View("Create", service);
        }


        //// POST: Service/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, Service service)
        //{
        //    if (id != service.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            service.UpdatedAt = DateTime.Now;
        //            _context.Update(service);
        //            await _context.SaveChangesAsync();

        //            TempData["Success"] = "Service updated successfully.";
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!ServiceExists(service.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }

        //    ViewBag.ServiceCategories = await _context.ServiceCategories
        //        .Where(c => c.IsActive)
        //        .OrderBy(c => c.DisplayOrder)
        //        .ToListAsync();

        //    return View(service);
        //}

        // GET: Service/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var service = await _context.Services.FindAsync(id);
                if (service == null)
                {
                    return NotFound();
                }

                // Soft delete - mark as inactive
                service.IsActive = false;
                service.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Service deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting service: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }





        //// POST: Service/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var service = await _context.Services.FindAsync(id);
        //    if (service != null)
        //    {
        //        // Soft delete - just mark as inactive
        //        service.IsActive = false;
        //        service.UpdatedAt = DateTime.Now;
        //        await _context.SaveChangesAsync();
                
        //        TempData["Success"] = "Service deactivated successfully.";
        //    }

        //    return RedirectToAction(nameof(Index));
        //}

        // GET: ServiceCategory
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.ServiceCategories
                .Include(c => c.Services.Where(s => s.IsActive))
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
            
            return View(categories);
        }

        // GET: ServiceCategory/Create
        public IActionResult CreateCategory()
        {
            return View();
        }

        // POST: ServiceCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(ServiceCategory category)
        {
            if (ModelState.IsValid)
            {
                category.CreatedAt = DateTime.Now;
                category.UpdatedAt = DateTime.Now;
                
                _context.Add(category);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Service category created successfully.";
                return RedirectToAction(nameof(Categories));
            }
            
            return View(category);
        }

        // GET: ServiceCategory/Edit/5
        public async Task<IActionResult> EditCategory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.ServiceCategories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            
            return View(category);
        }

        // POST: ServiceCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, ServiceCategory category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    category.UpdatedAt = DateTime.Now;
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = "Service category updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceCategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Categories));
            }
            
            return View(category);
        }

        // POST: ServiceCategory/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.ServiceCategories.FindAsync(id);
            if (category != null)
            {
                // Check if category has active services
                var hasActiveServices = await _context.Services
                    .AnyAsync(s =>s.IsActive);
                
                if (hasActiveServices)
                {
                    TempData["Error"] = "Cannot delete category with active services.";
                    return RedirectToAction(nameof(Categories));
                }
                
                // Soft delete - just mark as inactive
                category.IsActive = false;
                category.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Service category deactivated successfully.";
            }

            return RedirectToAction(nameof(Categories));
        }

        // API endpoint to calculate service price
        [HttpPost]
        public async Task<IActionResult> CalculatePrice([FromBody] ServicePriceRequest request)
        {
            try
            {
                var service = await _context.Services.FindAsync(request.ServiceId);
                if (service == null)
                {
                    return Json(new { success = false, error = "Service not found" });
                }

                var calculation = ServicePricingCalculator.CalculateBookingItem(
                    service, 
                    request.Quantity, 
                    request.WorkforceSize, 
                    request.Location, 
                    request.ServiceConfiguration
                );

                return Json(new { success = true, calculation });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.Id == id);
        }

        private bool ServiceCategoryExists(int id)
        {
            return _context.ServiceCategories.Any(e => e.Id == id);
        }

        // Manual seed data endpoint
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Manager,Secretary,Admin")]
        public async Task<IActionResult> SeedData()
        {
            try
            {
                await SeedServices.SeedBookUsDataAsync(_context);
                TempData["Success"] = "Service data seeded successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error seeding data: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // Fix CompanyCal schema issues
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Manager,Secretary,Admin")]
        public async Task<IActionResult> FixCompanyCalSchema()
        {
            try
            {
                var messages = new List<string>();

                // Step 1: Drop foreign key constraints
                await _context.Database.ExecuteSqlRawAsync(@"
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CompanyCals_Customers_CustomerId')
                    BEGIN
                        ALTER TABLE [CompanyCals] DROP CONSTRAINT [FK_CompanyCals_Customers_CustomerId];
                    END"
                );

                await _context.Database.ExecuteSqlRawAsync(@"
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CompanyCals_Invoices_InvoiceId')
                    BEGIN
                        ALTER TABLE [CompanyCals] DROP CONSTRAINT [FK_CompanyCals_Invoices_InvoiceId];
                    END"
                );
                messages.Add("✓ Dropped existing foreign key constraints");

                // Step 2: Drop shadow columns if they exist
                await _context.Database.ExecuteSqlRawAsync(@"
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[CompanyCals]') AND name = 'CustomerId1')
                    BEGIN
                        ALTER TABLE [CompanyCals] DROP COLUMN [CustomerId1];
                    END"
                );

                await _context.Database.ExecuteSqlRawAsync(@"
                    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[CompanyCals]') AND name = 'InvoiceId1')
                    BEGIN
                        ALTER TABLE [CompanyCals] DROP COLUMN [InvoiceId1];
                    END"
                );
                messages.Add("✓ Dropped shadow columns");

                // Step 3: Clean up data and convert to INT
                await _context.Database.ExecuteSqlRawAsync(@"
                    UPDATE [CompanyCals] 
                    SET [CustomerId] = NULL 
                    WHERE [CustomerId] IS NOT NULL 
                    AND (ISNUMERIC([CustomerId]) = 0 OR [CustomerId] = '')"
                );

                await _context.Database.ExecuteSqlRawAsync(@"
                    UPDATE [CompanyCals] 
                    SET [InvoiceId] = NULL 
                    WHERE [InvoiceId] IS NOT NULL 
                    AND (ISNUMERIC([InvoiceId]) = 0 OR [InvoiceId] = '')"
                );
                messages.Add("✓ Cleaned up invalid data");

                // Step 4: Change column types to INT
                await _context.Database.ExecuteSqlRawAsync(@"
                    IF EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'CompanyCals' 
                        AND COLUMN_NAME = 'CustomerId' 
                        AND DATA_TYPE IN ('varchar', 'nvarchar', 'char', 'nchar')
                    )
                    BEGIN
                        ALTER TABLE [CompanyCals] ALTER COLUMN [CustomerId] INT NULL;
                    END"
                );

                await _context.Database.ExecuteSqlRawAsync(@"
                    IF EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'CompanyCals' 
                        AND COLUMN_NAME = 'InvoiceId' 
                        AND DATA_TYPE IN ('varchar', 'nvarchar', 'char', 'nchar')
                    )
                    BEGIN
                        ALTER TABLE [CompanyCals] ALTER COLUMN [InvoiceId] INT NULL;
                    END"
                );
                messages.Add("✓ Changed column types to INT");

                // Step 5: Add proper foreign key constraints
                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CompanyCals_Customers_CustomerId')
                    AND EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
                    BEGIN
                        ALTER TABLE [CompanyCals] 
                        ADD CONSTRAINT [FK_CompanyCals_Customers_CustomerId] 
                        FOREIGN KEY ([CustomerId]) REFERENCES [Customers]([Id]);
                    END"
                );

                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_CompanyCals_Invoices_InvoiceId')
                    AND EXISTS (SELECT * FROM sys.tables WHERE name = 'Invoices')
                    BEGIN
                        ALTER TABLE [CompanyCals] 
                        ADD CONSTRAINT [FK_CompanyCals_Invoices_InvoiceId] 
                        FOREIGN KEY ([InvoiceId]) REFERENCES [Invoices]([Id]);
                    END"
                );
                messages.Add("✓ Added proper foreign key constraints");

                TempData["Success"] = "CompanyCal schema fixed successfully! Member dashboard should now work properly. " + string.Join(" | ", messages);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error fixing CompanyCal schema: {ex.Message}. Details: {ex.InnerException?.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // Fix ServiceCategoryId constraint
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Manager,Secretary,Admin")]
        public async Task<IActionResult> FixServiceCategoryConstraint()
        {
            try
            {
                var messages = new List<string>();

                // Step 1: Drop foreign key constraint if it exists
                await _context.Database.ExecuteSqlRawAsync(@"
                    IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Services_ServiceCategories_ServiceCategoryId')
                    BEGIN
                        ALTER TABLE [Services] DROP CONSTRAINT [FK_Services_ServiceCategories_ServiceCategoryId];
                    END"
                );
                messages.Add("✓ Foreign key constraint removed (if existed)");

                // Step 2: Make ServiceCategoryId nullable if it's not already
                await _context.Database.ExecuteSqlRawAsync(@"
                    IF EXISTS (
                        SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'Services' 
                        AND COLUMN_NAME = 'ServiceCategoryId' 
                        AND IS_NULLABLE = 'NO'
                    )
                    BEGIN
                        ALTER TABLE [Services] ALTER COLUMN [ServiceCategoryId] INT NULL;
                    END"
                );
                messages.Add("✓ ServiceCategoryId column set to allow NULL values");

                // Step 3: Update any invalid ServiceCategoryId values to NULL
                await _context.Database.ExecuteSqlRawAsync(@"
                    UPDATE [Services] 
                    SET [ServiceCategoryId] = NULL 
                    WHERE [ServiceCategoryId] IS NOT NULL 
                    AND ([ServiceCategoryId] = 0 OR [ServiceCategoryId] NOT IN (SELECT Id FROM ServiceCategories WHERE Id IS NOT NULL))"
                );
                messages.Add("✓ Invalid ServiceCategoryId values set to NULL");

                TempData["Success"] = "ServiceCategoryId constraint fixed successfully! You can now create services without category errors. " + string.Join(" | ", messages);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error fixing constraint: {ex.Message}. Details: {ex.InnerException?.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        // GET: Service/Bookings
        public async Task<IActionResult> Bookings(int page = 1, string status = "", string search = "")
        {
            var query = _context.Bookings
                .Include(b => b.BookingItems)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.BookingStatus == status);
            }

            // Search functionality
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => 
                    b.CustomerName.Contains(search) ||
                    b.CustomerEmail.Contains(search) ||
                    b.BookingNumber.Contains(search) ||
                    b.CompanyName.Contains(search));
            }

            // Order by booking date descending
            query = query.OrderByDescending(b => b.BookingDate);

            // Pagination
            var pageSize = 10;
            var totalCount = await query.CountAsync();
            var bookings = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new BookingListViewModel
            {
                Bookings = bookings,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Status = status,
                Search = search,
                TotalCount = totalCount
            };

            return View(viewModel);
        }

        // GET: Service/BookingDetails/{id}
        public async Task<IActionResult> BookingDetails(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingItems)
                .ThenInclude(bi => bi.Service)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Service/UpdateBookingStatus
        [HttpPost]
        public async Task<IActionResult> UpdateBookingStatus(int bookingId, string status)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null)
                {
                    return Json(new { success = false, message = "Booking not found" });
                }

                booking.BookingStatus = status;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Booking status updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Service/VerifyPayment
        [HttpPost]
        public async Task<IActionResult> VerifyPayment(int bookingId)
        {
            try
            {
                var booking = await _context.Bookings.FindAsync(bookingId);
                if (booking == null)
                {
                    return Json(new { success = false, message = "Booking not found" });
                }

                booking.PaymentStatus = "Paid";
                booking.BookingStatus = "Confirmed";
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Payment verified successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

}
