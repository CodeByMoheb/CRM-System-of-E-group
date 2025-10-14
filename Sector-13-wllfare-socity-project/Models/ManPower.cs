using Microsoft.AspNetCore.Mvc.Rendering;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class ManPower : Base
    {
        public string? ManPowerType { get; set; }
        public int? ServiceId { get; set; }
        public decimal? ManPowerDay { get; set; }
        public decimal? ManPowerPrice { get; set; }

        public Service? Service { get; set; }
    }
    public class ManPowerViewModel
    {
        public ManPower ManPower { get; set; } = new ManPower();
        public List<ManPower> ManPowerList { get; set; } = new List<ManPower>();

        public IEnumerable<SelectListItem> Services { get; set; } = new List<SelectListItem>();
    }

}
