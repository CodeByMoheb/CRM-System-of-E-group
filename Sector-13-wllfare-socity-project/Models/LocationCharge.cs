namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class LocationCharge : Base
    {
        public string? LChargeType { get; set; }
        public decimal? LChargeValue { get; set; }
    }
    public class LocationChargeViewModel
    {
        public LocationCharge LocationCharge { get; set; } = new LocationCharge();
        public List<LocationCharge> LocationChargeList { get; set; } = new List<LocationCharge>();
    }

}
