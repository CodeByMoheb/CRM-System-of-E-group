using Microsoft.EntityFrameworkCore;
using Sector_13_Welfare_Society___Digital_Management_System.Data;
using Sector_13_Welfare_Society___Digital_Management_System.Models;

namespace Sector_13_Welfare_Society___Digital_Management_System
{
    public static class SeedServices
    {
        public static async Task SeedBookUsDataAsync(ApplicationDbContext context)
        {
            // Check if data already exists
            if (await context.ServiceCategories.AnyAsync())
            {
                Console.WriteLine("Service data already exists. Skipping seed.");
                return;
            }

            // Create Service Categories
            var categories = new List<ServiceCategory>
            {
                new ServiceCategory
                {
                    Name = "Social Compliance",
                    Description = "Social compliance and ethical trading audits",
                    IconClass = "fas fa-shield-alt",
                    DisplayOrder = 1,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Environmental",
                    Description = "Environmental management and sustainability certifications",
                    IconClass = "fas fa-leaf",
                    DisplayOrder = 2,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Quality",
                    Description = "Quality management system certifications",
                    IconClass = "fas fa-award",
                    DisplayOrder = 3,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Health & Safety",
                    Description = "Occupational health and safety management",
                    IconClass = "fas fa-user-shield",
                    DisplayOrder = 4,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },
                new ServiceCategory
                {
                    Name = "Textile Standards",
                    Description = "Textile and organic certification standards",
                    IconClass = "fas fa-tshirt",
                    DisplayOrder = 5,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            };

            context.ServiceCategories.AddRange(categories);
            await context.SaveChangesAsync();
            Console.WriteLine($"Created {categories.Count} service categories.");

            // Create Services
            var services = new List<Service>
            {
                // BSCI Service
                new Service
                {
                    Name = "BSCI Audit",
                    Description = "Business Social Compliance Initiative audit to ensure ethical working conditions and social responsibility in your supply chain.",
                    ServiceType = "BSCI",
                    
                    Currency = "USD",
                    IsActive = true,
                    
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Name = "SMETA Audit",
                    Description = "Sedex Members Ethical Trade Audit for ethical trading practices.",
                    ServiceType = "SMETA",
            
                    Currency = "USD",
                    IsActive = true,
              
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Name = "SLCP Audit",
                    Description = "Social & Labor Convergence Program audit for social compliance.",
                    ServiceType = "SLCP",
            
                    Currency = "USD",
                    IsActive = true,
          
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Name = "Higg FSLM",
                    Description = "Higg Facility Social & Labor Module assessment.",
                    ServiceType = "HIGG FSLM",
              
                    Currency = "USD",
                    IsActive = true,
          
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Name = "WRAP Certification",
                    Description = "Worldwide Responsible Accredited Production certification for ethical manufacturing.",
                    ServiceType = "WRAP",
  
                    Currency = "USD",
                    IsActive = true,
              
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Name = "GOTS Certification",
                    Description = "Global Organic Textile Standard certification for organic textiles.",
                    ServiceType = "GOTS",
         
                    Currency = "USD",
                    IsActive = true,
         
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Name = "OCS Certification",
                    Description = "Organic Content Standard certification for organic content verification.",
                    ServiceType = "OCS",

                    Currency = "USD",
                    IsActive = true,
           
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Name = "RCS Certification",
                    Description = "Recycled Claim Standard certification for recycled content verification.",
                    ServiceType = "RCS",
              
                    Currency = "USD",
                    IsActive = true,
            
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Name = "GRS Certification",
                    Description = "Global Recycled Standard certification for recycled content verification.",
                    ServiceType = "GRS",
            
                    Currency = "USD",
                    IsActive = true,
                 
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Name = "GRS+RCS Combined",
                    Description = "Combined Global Recycled Standard and Recycled Claim Standard certification.",
                    ServiceType = "GRS+RCS",
                  
                    Currency = "USD",
                    IsActive = true,
                
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Name = "GOTS+OCS+RCS+GRS Combined",
                    Description = "Combined certification for GOTS, OCS, RCS, and GRS standards.",
                    ServiceType = "GOTS+OCS+RCS+GRS",
                
                    Currency = "USD",
                    IsActive = true,
               
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Name = "GOTS+OCS Combined",
                    Description = "Combined GOTS and OCS certification for organic content verification.",
                    ServiceType = "GOTS+OCS",
                
                    Currency = "USD",
                    IsActive = true,
                 
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Name = "ISO 9001 Certification",
                    Description = "ISO 9001 Quality Management System certification.",
                    ServiceType = "ISO9001",
             
                    Currency = "USD",
                    IsActive = true,
       
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Name = "ISO 14001 Certification",
                    Description = "ISO 14001 Environmental Management System certification.",
                    ServiceType = "ISO14001",
                  
                    Currency = "USD",
                    IsActive = true,
                
                    CreatedAt = DateTime.Now
                },
                new Service
                {
                    Name = "ISO 45001 Certification",
                    Description = "ISO 45001 Occupational Health and Safety Management System certification.",
                    ServiceType = "ISO45001",
             
                    Currency = "USD",
                    IsActive = true,
               
                    CreatedAt = DateTime.Now
                }
            };

            context.Services.AddRange(services);
            await context.SaveChangesAsync();
            Console.WriteLine($"Created {services.Count} services.");
        }
    }
}