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
        public List<CheckModel>? Checks { get; set; }
        public string? Icon { get; set; }
    }
}
