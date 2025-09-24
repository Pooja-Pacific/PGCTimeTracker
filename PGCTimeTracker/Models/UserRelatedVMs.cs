using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PGCTimeTracker_V2.Models
{
    public class LoginResponseVM
    {
        public int UserId { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public int RoleId { get; set; }
        public List<OrganizationVM> Organizations { get; set; } = new();
    }
    public class UserCredentials
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class UserLogoutVM
    {
        public DateTime? LogOutTime { get; set; }
        public long UserId { get; set; }
    }
    public class LogoutTimeVM
    {
        public DateTime? LogOutTime { get; set; }
    }
}
