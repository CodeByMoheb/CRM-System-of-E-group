using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;

namespace Sector_13_Welfare_Society___Digital_Management_System.Controllers
{
    public class DatabaseFixController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DatabaseFixController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> FixPaymentRecords()
        {
            try
            {
                // Check if PaymentRecords table exists
                var tableExists = await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentRecords')
                    BEGIN
                        CREATE TABLE PaymentRecords (
                            Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            Amount decimal(18,2) NOT NULL,
                            Currency nvarchar(10) NOT NULL,
                            PaymentMethod nvarchar(50) NOT NULL,
                            Status nvarchar(50) NOT NULL,
                            TransactionId nvarchar(100) NULL,
                            PaymentProofUrl nvarchar(max) NULL,
                            Notes nvarchar(max) NULL,
                            VerifiedAt datetime2 NULL,
                            VerifiedBy nvarchar(max) NULL,
                            OrderId int NOT NULL,
                            CreatedAt datetime2 NOT NULL,
                            UpdatedAt datetime2 NOT NULL,
                            CreatedBy nvarchar(max) NULL,
                            ApprovedBy nvarchar(max) NULL,
                            ApprovedAt datetime2 NOT NULL,
                            IsDelete bit NOT NULL DEFAULT 0,
                            RequiresApproval bit NOT NULL DEFAULT 0,
                            ApprovalStatus nvarchar(50) NULL,
                            BookingId int NULL,
                            CustomerName nvarchar(255) NOT NULL DEFAULT '',
                            CustomerEmail nvarchar(255) NOT NULL DEFAULT '',
                            CustomerPhone nvarchar(50) NOT NULL DEFAULT '',
                            RejectedAt datetime2 NULL,
                            RejectedBy nvarchar(255) NULL,
                            RejectionReason nvarchar(1000) NULL
                        );
                    END
                ");

                // Add missing columns if table exists
                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PaymentRecords') AND name = 'RequiresApproval')
                    BEGIN
                        ALTER TABLE PaymentRecords ADD RequiresApproval bit NOT NULL DEFAULT 0;
                    END
                ");

                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PaymentRecords') AND name = 'ApprovalStatus')
                    BEGIN
                        ALTER TABLE PaymentRecords ADD ApprovalStatus nvarchar(50) NULL;
                    END
                ");

                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PaymentRecords') AND name = 'BookingId')
                    BEGIN
                        ALTER TABLE PaymentRecords ADD BookingId int NULL;
                    END
                ");

                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PaymentRecords') AND name = 'CustomerName')
                    BEGIN
                        ALTER TABLE PaymentRecords ADD CustomerName nvarchar(255) NOT NULL DEFAULT '';
                    END
                ");

                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PaymentRecords') AND name = 'CustomerEmail')
                    BEGIN
                        ALTER TABLE PaymentRecords ADD CustomerEmail nvarchar(255) NOT NULL DEFAULT '';
                    END
                ");

                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PaymentRecords') AND name = 'CustomerPhone')
                    BEGIN
                        ALTER TABLE PaymentRecords ADD CustomerPhone nvarchar(50) NOT NULL DEFAULT '';
                    END
                ");

                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PaymentRecords') AND name = 'RejectedAt')
                    BEGIN
                        ALTER TABLE PaymentRecords ADD RejectedAt datetime2 NULL;
                    END
                ");

                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PaymentRecords') AND name = 'RejectedBy')
                    BEGIN
                        ALTER TABLE PaymentRecords ADD RejectedBy nvarchar(255) NULL;
                    END
                ");

                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PaymentRecords') AND name = 'RejectionReason')
                    BEGIN
                        ALTER TABLE PaymentRecords ADD RejectionReason nvarchar(1000) NULL;
                    END
                ");

                // Add foreign key constraint for BookingId
                await _context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PaymentRecords_Bookings_BookingId')
                    BEGIN
                        ALTER TABLE PaymentRecords 
                        ADD CONSTRAINT FK_PaymentRecords_Bookings_BookingId 
                        FOREIGN KEY (BookingId) REFERENCES Bookings(Id) ON DELETE CASCADE;
                    END
                ");

                TempData["Success"] = "PaymentRecords table has been fixed successfully!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error fixing database: {ex.Message}";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}