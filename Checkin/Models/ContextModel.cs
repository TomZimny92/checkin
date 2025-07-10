using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkin.Models
{
    public class ContextModel
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public Boolean CheckedIn { get; set; }
        public List<CheckModel>? Checks { get; set; }
        public string? Icon { get; set; }
        public string? Duration { get; set; }
        public bool CheckinButtonEnabled { get; set;}
        public bool CheckoutButtonEnabled { get; set; }
        public string CheckinButtonColor { get; set; }
        public string CheckoutButtonColor { get; set; }
        
    }
}
