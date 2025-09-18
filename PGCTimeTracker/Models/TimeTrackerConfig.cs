using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGCTimeTracker.Models
{
    public class TimeTrackerConfig
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string RoleName { get; set; } = "";
        public string OrgToken { get; set; } = "";
    }
}
