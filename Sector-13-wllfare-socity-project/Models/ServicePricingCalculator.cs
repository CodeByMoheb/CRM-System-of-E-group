using System.ComponentModel.DataAnnotations.Schema;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public static class ServicePricingCalculator
    {
        public static BookingItemCalculation CalculateBookingItem(Service service, int quantity = 1,
            int? workforceSize = null, string? location = null, string? serviceConfiguration = null)
        {
            var calculation = new BookingItemCalculation
            {
                ServiceId = service.Id,
                ServiceName = service.Name,
                ServiceDescription = service.Description,
                ServiceType = service.ServiceType,
                Quantity = quantity,
                Currency = service.Currency
            };

            // Calculate the total amount using the new core logic
            var totalAmount = CalculateAuditFee(
                service.ServiceType, 
                workforceSize ?? 100, 
                location ?? "Inside Dhaka", 
                serviceConfiguration
            );

            calculation.UnitPrice = totalAmount;
            calculation.Subtotal = totalAmount * quantity;
            calculation.TotalAmount = calculation.Subtotal;

            // For BSCI services, show breakdown
            if (IsBSCIService(service.ServiceType))
            {
                var breakdown = GetBSCIBreakdown(service.ServiceType, workforceSize ?? 100, location ?? "Inside Dhaka", serviceConfiguration);
                calculation.TravelAllowance = breakdown.TravelAllowance;
                calculation.VatAmount = breakdown.VatAmount;
                calculation.ManDaysRequired = breakdown.ManDaysRequired;
            }

            return calculation;
        }

        public static decimal CalculateAuditFee(string auditType, int workers, string location, string? isoOption = null)
        {
            // Step 1: Base Man-Day Calculation
            int baseManDays = GetBaseManDays(workers);

            // Step 2: Travel Cost
            decimal travel = GetTravelCost(location);

            // Step 3: Initialize
            decimal auditFee = 0;
            decimal registrationFee = 0;
            int manDays = baseManDays;

            // Step 4: Audit Type Logic
            switch (auditType.ToUpper())
            {
                case "BSCI":
                case "SMETA":
                case "SLCP":
                case "HIGG FSLM":
                    auditFee = manDays * 300;
                    break;

                case "WRAP":
                    auditFee = manDays * 400;
                    break;

                case "GOTS":
                    auditFee = manDays * 300;
                    registrationFee = 200;
                    break;

                case "OCS":
                    auditFee = manDays * 300;
                    registrationFee = 200;
                    break;

                case "RCS":
                    auditFee = manDays * 300;
                    registrationFee = 200;
                    break;

                case "GRS":
                    auditFee = manDays * 300;
                    registrationFee = 300;
                    break;

                case "GRS+RCS":
                    manDays = GetCombinedManDays(workers);
                    auditFee = manDays * 300;
                    registrationFee = 500; // GRS(300) + RCS(200)
                    break;

                case "GOTS+OCS+RCS+GRS":
                    manDays = GetCombinedManDays(workers);
                    auditFee = manDays * 300;
                    registrationFee = 900; // 200+200+200+300
                    break;

                case "GOTS+OCS":
                    manDays = GetCombinedManDays(workers);
                    auditFee = manDays * 300;
                    registrationFee = 400; // 200+200
                    break;

                case "ISO9001":
                case "ISO14001":
                case "ISO45001":
                    return CalculateISOFee(workers, location, isoOption);

                default:
                    // Default calculation for unknown audit types
                    auditFee = manDays * 300;
                    break;
            }

            // Step 5: Default Calculation (non-ISO)
            decimal subtotal = auditFee + registrationFee + travel;
            decimal vat = subtotal * 0.15m;
            decimal total = subtotal + vat;

            return total;
        }

        private static int GetBaseManDays(int workers)
        {
            if (workers < 100) return 1;
            if (workers <= 500) return 2;
            if (workers <= 1000) return 3;
            return 4;
        }

        private static int GetCombinedManDays(int workers)
        {
            if (workers < 100) return 2;
            if (workers <= 500) return 3;
            if (workers <= 1000) return 4;
            return 5;
        }

        private static decimal GetTravelCost(string location)
        {
            return location?.ToLower().Contains("inside") == true ? 40 : 55;
        }

        private static decimal CalculateISOFee(int workers, string location, string? isoOption)
        {
            // ISO Special Rule
            int manDays;
            if (workers < 100) manDays = 4;
            else if (workers <= 500) manDays = 6;
            else if (workers <= 1000) manDays = 8;
            else manDays = 10;

            if (isoOption == "Option1")
            {
                decimal auditFee = manDays * 120;
                decimal travel = GetTravelCost(location);
                decimal subtotal = auditFee + travel;
                decimal vat = subtotal * 0.15m;
                return subtotal + vat;
            }
            else if (isoOption == "Option2")
            {
                return 2880; // fixed (80*36 months, travel excluded)
            }

            // Default ISO calculation
            decimal defaultAuditFee = manDays * 120;
            decimal defaultTravel = GetTravelCost(location);
            decimal defaultSubtotal = defaultAuditFee + defaultTravel;
            decimal defaultVat = defaultSubtotal * 0.15m;
            return defaultSubtotal + defaultVat;
        }

        private static bool IsBSCIService(string serviceType)
        {
            var bsciservices = new[] { "BSCI", "SMETA", "SLCP", "HIGG FSLM", "WRAP", "GOTS", "OCS", "RCS", "GRS" };
            return bsciservices.Any(s => serviceType.ToUpper().Contains(s));
        }

        private static BSCIBreakdown GetBSCIBreakdown(string auditType, int workers, string location, string? isoOption)
        {
            int manDays = GetBaseManDays(workers);
            decimal travel = GetTravelCost(location);
            decimal auditFee = 0;
            decimal registrationFee = 0;

            switch (auditType.ToUpper())
            {
                case "BSCI":
                case "SMETA":
                case "SLCP":
                case "HIGG FSLM":
                    auditFee = manDays * 300;
                    break;
                case "WRAP":
                    auditFee = manDays * 400;
                    break;
                case "GOTS":
                case "OCS":
                case "RCS":
                    auditFee = manDays * 300;
                    registrationFee = 200;
                    break;
                case "GRS":
                    auditFee = manDays * 300;
                    registrationFee = 300;
                    break;
                case "GRS+RCS":
                    manDays = GetCombinedManDays(workers);
                    auditFee = manDays * 300;
                    registrationFee = 500;
                    break;
                case "GOTS+OCS+RCS+GRS":
                    manDays = GetCombinedManDays(workers);
                    auditFee = manDays * 300;
                    registrationFee = 900;
                    break;
                case "GOTS+OCS":
                    manDays = GetCombinedManDays(workers);
                    auditFee = manDays * 300;
                    registrationFee = 400;
                    break;
            }

            decimal subtotal = auditFee + registrationFee + travel;
            decimal vat = subtotal * 0.15m;

            return new BSCIBreakdown
            {
                ManDaysRequired = manDays,
                TravelAllowance = travel,
                VatAmount = vat
            };
        }
    }

    public class BookingItemCalculation
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? ServiceDescription { get; set; }
        public string ServiceType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public decimal? TravelAllowance { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "USD";
        public int? WorkforceSize { get; set; }
        public int? ManDaysRequired { get; set; }
        public string? Location { get; set; }
        public string? ServiceConfiguration { get; set; }
    }

    public class BSCIBreakdown
    {
        public int ManDaysRequired { get; set; }
        public decimal TravelAllowance { get; set; }
        public decimal VatAmount { get; set; }
    }
}