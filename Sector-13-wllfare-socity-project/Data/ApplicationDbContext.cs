using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<ApprovalRequest> ApprovalRequests { get; set; }
        public DbSet<Donor> Donors { get; set; }

        public new DbSet<Role> Roles { get; set; }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<LeaveBalance> LeaveBalances { get; set; }
        public DbSet<LeaveEntitlementPolicy> LeaveEntitlementPolicies { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<PermanentMember> PermanentMembers { get; set; }
        public DbSet<GalleryImage> GalleryImages { get; set; }

        public DbSet<ClientsServices> ClientsServices { get; set; }
       

        // Book Us System
        public DbSet<ServiceCategory> ServiceCategories { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingItem> BookingItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BuyerContact> BuyerContacts { get; set; }
        public DbSet<ClientContact> ClientContacts { get; set; }
        

        public DbSet<ManPower> ManPowers { get; set; }
        public DbSet<LocationCharge> LocationCharges { get; set; }
        public DbSet<CompanyCal> CompanyCals { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        
        // Cart and Order System
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<PaymentRecord> PaymentRecords { get; set; }
        
        // Audit System
        public DbSet<AuditQuestion> AuditQuestions { get; set; }
        public DbSet<AuditResponse> AuditResponses { get; set; }
        public DbSet<AuditSession> AuditSessions { get; set; }
        public DbSet<CorrectiveActionPlan> CorrectiveActionPlans { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configure ApprovalRequest
            builder.Entity<ApprovalRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.RequestType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RequestedBy).IsRequired();
                entity.Property(e => e.RequestedByName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RequestDate).IsRequired();
                entity.Property(e => e.Status).IsRequired();
            });
            
            // Configure Donor
            builder.Entity<Donor>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.PaymentStatus).HasMaxLength(20);
                entity.Property(e => e.DonationDate).IsRequired();
                entity.Property(e => e.Message).HasMaxLength(500);
                entity.Property(e => e.DonationType).HasMaxLength(50);
                entity.Property(e => e.ReceiptNumber).HasMaxLength(50);
            });

            // Configure Role
            builder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // Configure Employee
            builder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EmployeeId).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.EmployeeId).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.RoleId).IsRequired(false);
                entity.Property(e => e.BaseSalary).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                    
                entity.HasOne(e => e.Shift)
                    .WithMany()
                    .HasForeignKey(e => e.ShiftId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure Attendance
            builder.Entity<Attendance>(entity =>
            {
                entity.HasKey(e => e.AttendanceId);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.TotalHours).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.Remarks).HasMaxLength(500);
            });

            // Configure Shift
            builder.Entity<Shift>(entity =>
            {
                entity.HasKey(e => e.ShiftId);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
            });

            // Configure Leave
            builder.Entity<Leave>(entity =>
            {
                entity.HasKey(e => e.LeaveId);
                entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Reason).HasMaxLength(500);
                entity.Property(e => e.ApprovalStatus).IsRequired().HasMaxLength(20);
                entity.Property(e => e.ApprovalRemarks).HasMaxLength(200);
            });

            // Configure LeaveBalance
            builder.Entity<LeaveBalance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TotalEntitled).IsRequired();
                entity.Property(e => e.Used).IsRequired();
                entity.Property(e => e.Pending).IsRequired();
                entity.Property(e => e.Remaining).IsRequired();
                entity.Property(e => e.Year).IsRequired();
                
                entity.HasOne(e => e.Employee)
                    .WithMany()
                    .HasForeignKey(e => e.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure LeaveEntitlementPolicy
            builder.Entity<LeaveEntitlementPolicy>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LeaveType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.DefaultEntitled).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(200);
                entity.Property(e => e.IsActive).IsRequired();
            });
            
            // Configure ServiceCategory
            builder.Entity<ServiceCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.IconClass).HasMaxLength(200);
                entity.Property(e => e.CreatedAt).IsRequired();
            });
            
            // Configure Service
            builder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.ServiceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BasePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Registration_fees).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Currency).HasMaxLength(10);
                entity.Property(e => e.CompanyCalId).HasMaxLength(50);
                entity.Property(e => e.CreatedAt).IsRequired();
                //entity.Property(e => e.ManDayRate).HasColumnType("decimal(18,2)");
                //entity.Property(e => e.InsideDhakaTravelAllowance).HasColumnType("decimal(18,2)");
                //entity.Property(e => e.OutsideDhakaTravelAllowance).HasColumnType("decimal(18,2)");
                //entity.Property(e => e.VatRate).HasColumnType("decimal(5,4)");
                //entity.Property(e => e.PricingConfiguration).HasMaxLength(2000);
                
                // Removed ServiceCategory relationship - not needed
                // entity.HasOne(e => e.ServiceCategory)
                //     .WithMany(c => c.Services)
                //     .HasForeignKey(e => e.ServiceCategoryId)
                //     .OnDelete(DeleteBehavior.SetNull);
            });
            
            // Configure Booking
            builder.Entity<Booking>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.BookingNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.BookingNumber).IsUnique();
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(150);
                entity.Property(e => e.CustomerPhone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CustomerAddress).HasMaxLength(200);
                entity.Property(e => e.CompanyName).HasMaxLength(200);
                entity.Property(e => e.SpecialRequirements).HasMaxLength(500);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(10);
                entity.Property(e => e.PaymentMethod).IsRequired().HasMaxLength(50);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(20);
                entity.Property(e => e.BookingDate).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.BookingStatus).HasMaxLength(50);
            });
            
            // Configure BookingItem
            builder.Entity<BookingItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ServiceDescription).HasMaxLength(1000);
                entity.Property(e => e.ServiceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.TravelAllowance).HasColumnType("decimal(18,2)");
                entity.Property(e => e.VatAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.Currency).HasMaxLength(10);
                entity.Property(e => e.Location).HasMaxLength(50);
                entity.Property(e => e.ServiceConfiguration).HasMaxLength(2000);
                
                entity.HasOne(e => e.Booking)
                    .WithMany(b => b.BookingItems)
                    .HasForeignKey(e => e.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Service)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Member Directory: BuyerContact
            builder.Entity<BuyerContact>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(300);
                entity.Property(e => e.ContactPerson).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Designation).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(30);
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // Member Directory: ClientContact
            builder.Entity<ClientContact>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Country).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(300);
                entity.Property(e => e.ContactPerson).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Designation).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Phone).HasMaxLength(30);
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // Configure CompanyCal
            builder.Entity<CompanyCal>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.VAT).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Discount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaymentStatus).HasMaxLength(50);
                
                // Configure foreign key relationships
                entity.HasOne(e => e.Customer)
                    .WithMany()
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.Invoice)
                    .WithMany()
                    .HasForeignKey(e => e.InvoiceId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.LocationCharge)
                    .WithMany()
                    .HasForeignKey(e => e.LocationChargeId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.ManPower)
                    .WithMany()
                    .HasForeignKey(e => e.ManPowerId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.Service)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure PaymentRecord
            builder.Entity<PaymentRecord>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Currency).HasMaxLength(10);
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.RequiresApproval).IsRequired();
                entity.Property(e => e.ApprovalStatus).HasMaxLength(50);
                entity.Property(e => e.CustomerName).HasMaxLength(255);
                entity.Property(e => e.CustomerEmail).HasMaxLength(255);
                entity.Property(e => e.CustomerPhone).HasMaxLength(50);
                entity.Property(e => e.RejectionReason).HasMaxLength(1000);
                entity.Property(e => e.RejectedBy).HasMaxLength(255);
                entity.Property(e => e.PaymentProofUrl).HasMaxLength(1000);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                
                entity.HasOne(e => e.Order)
                    .WithMany()
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Booking)
                    .WithMany()
                    .HasForeignKey(e => e.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Order and OrderItem
            builder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).HasMaxLength(50);
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.VatAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Currency).HasMaxLength(10);
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.PaymentStatus).HasMaxLength(50);
                entity.Property(e => e.OrderStatus).HasMaxLength(50);
                
                // Configure relationship with ApplicationUser
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
                
                entity.HasMany(e => e.OrderItems)
                    .WithOne(oi => oi.Order)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TravelAllowance).HasColumnType("decimal(18,2)");
                entity.Property(e => e.VatAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Currency).HasMaxLength(10);
                
                entity.HasOne(e => e.Service)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure LocationCharge
            builder.Entity<LocationCharge>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.LChargeValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.LChargeType).HasMaxLength(100);
            });

            // Configure ManPower
            builder.Entity<ManPower>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ManPowerDay).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ManPowerPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ManPowerType).HasMaxLength(100);
            });

            // Configure CartItem
            builder.Entity<CartItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ServiceDescription).HasMaxLength(1000);
                entity.Property(e => e.ServiceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TravelAllowance).HasColumnType("decimal(18,2)");
                entity.Property(e => e.VatAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Currency).HasMaxLength(10);
                entity.Property(e => e.Location).HasMaxLength(50);
                entity.Property(e => e.ServiceConfiguration).HasMaxLength(2000);
                entity.Property(e => e.SpecialRequirements).HasMaxLength(500);
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Service)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure Order
            builder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(150);
                entity.Property(e => e.CustomerPhone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CustomerAddress).HasMaxLength(200);
                entity.Property(e => e.CompanyName).HasMaxLength(200);
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.VatAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Currency).HasMaxLength(10);
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.PaymentStatus).HasMaxLength(50);
                entity.Property(e => e.OrderStatus).HasMaxLength(50);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure OrderItem
            builder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ServiceName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ServiceDescription).HasMaxLength(1000);
                entity.Property(e => e.ServiceType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TravelAllowance).HasColumnType("decimal(18,2)");
                entity.Property(e => e.VatAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Currency).HasMaxLength(10);
                entity.Property(e => e.Location).HasMaxLength(50);
                entity.Property(e => e.ServiceConfiguration).HasMaxLength(2000);
                entity.Property(e => e.SpecialRequirements).HasMaxLength(500);
                
                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Service)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        }
    }
}