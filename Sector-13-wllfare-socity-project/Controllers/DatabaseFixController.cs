using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class DatabaseFixController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DatabaseFixController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DatabaseFix/Index
        public IActionResult Index()
        {
            return View();
        }

        // POST: DatabaseFix/FixSchema
        [HttpPost]
        public async Task<IActionResult> FixSchema()
        {
            try
            {
                var messages = new List<string>();

                // Read and execute the comprehensive fix script
                var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "comprehensive_database_fix.sql");
                if (System.IO.File.Exists(scriptPath))
                {
                    var script = await System.IO.File.ReadAllTextAsync(scriptPath);
                    
                    // Split script into individual commands
                    var commands = script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (var command in commands)
                    {
                        if (!string.IsNullOrWhiteSpace(command))
                        {
                            try
                            {
                                await _context.Database.ExecuteSqlRawAsync(command);
                            }
                            catch (Exception ex)
                            {
                                // Log but continue with other commands
                                messages.Add($"⚠ Command warning: {ex.Message}");
                            }
                        }
                    }
                    
                    messages.Add("✓ Executed comprehensive database fix script");
                }
                else
                {
                    // Fallback to individual fixes
                    messages.Add("⚠ Script file not found, using fallback method");
                    
                    // Fix Orders table UserId
                    await _context.Database.ExecuteSqlRawAsync(@"
                        -- Create Orders table if not exists with correct schema
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
                        BEGIN
                            CREATE TABLE [Orders] (
                                [Id] int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                [OrderNumber] nvarchar(50) NOT NULL,
                                [UserId] nvarchar(450) NULL,
                                [CustomerName] nvarchar(255) NOT NULL,
                                [CustomerEmail] nvarchar(255) NOT NULL,
                                [CustomerPhone] nvarchar(50) NOT NULL,
                                [CustomerAddress] nvarchar(500) NULL,
                                [CompanyName] nvarchar(255) NULL,
                                [Subtotal] decimal(18,2) NOT NULL,
                                [VatAmount] decimal(18,2) NOT NULL,
                                [TotalAmount] decimal(18,2) NOT NULL,
                                [Currency] nvarchar(10) NOT NULL DEFAULT 'BDT',
                                [PaymentMethod] nvarchar(50) NOT NULL DEFAULT 'Pending',
                                [PaymentStatus] nvarchar(50) NOT NULL DEFAULT 'Pending',
                                [OrderStatus] nvarchar(50) NOT NULL DEFAULT 'Pending',
                                [PaymentDate] datetime2 NULL,
                                [TransactionId] nvarchar(100) NULL,
                                [Notes] nvarchar(1000) NULL,
                                [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
                                [UpdatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
                                [CreatedBy] nvarchar(450) NULL,
                                [ApprovedBy] nvarchar(450) NULL,
                                [ApprovedAt] datetime2 NOT NULL DEFAULT GETDATE(),
                                [IsApproved] bit NOT NULL DEFAULT 1,
                                [IsDelete] bit NOT NULL DEFAULT 0
                            );
                        END");
                    
                    messages.Add("✓ Created/verified Orders table");
                    
                    // Add foreign key if it doesn't exist
                    await _context.Database.ExecuteSqlRawAsync(@"
                        IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Orders_AspNetUsers_UserId')
                        AND EXISTS (SELECT * FROM sys.tables WHERE name = 'AspNetUsers')
                        AND EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders')
                        BEGIN
                            ALTER TABLE [Orders] ADD CONSTRAINT [FK_Orders_AspNetUsers_UserId] 
                            FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers]([Id]) ON DELETE SET NULL;
                        END");
                    
                    messages.Add("✓ Added Orders foreign key constraint");
                }

                TempData["Success"] = "Database schema fixed successfully! System should now work properly. " + string.Join(" | ", messages);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error fixing database schema: {ex.Message}. Inner: {ex.InnerException?.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
