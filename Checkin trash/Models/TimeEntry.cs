using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkin.Models
{
    public class TimeEntry
    {
        public DateTime CheckinTime { get; set; }
        public DateTime? CheckoutTime { get; set; }

        public TimeSpan Duration
        {
            get
            {
                if (CheckoutTime.HasValue)
                {
                    return CheckoutTime.Value - CheckinTime;
                }
                return TimeSpan.Zero; // Or handle as an ongoing session if needed elsewhere
            }
        }
    }
}
