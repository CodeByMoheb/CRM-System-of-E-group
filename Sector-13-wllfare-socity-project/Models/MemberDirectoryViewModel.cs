using System.Collections.Generic;

namespace Sector_13_Welfare_Society___Digital_Management_System.Models
{
    public class MemberDirectoryViewModel
    {
        public List<BuyerContact> Buyers { get; set; } = new List<BuyerContact>();
        public List<ClientContact> Clients { get; set; } = new List<ClientContact>();
    }
}


