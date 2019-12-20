using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Web.Models.ViewModels
{
    public class UserViewModel
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
        public string CurrentRoom { get; set; }
        public string Device { get; set; }
        public string UserId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyId { get; set; }
        public string Status { get; set; }
        public string UnReadMessage { get; set; }
    }

    public class CompanyViewModel
    {
        public string CompanyName { get; set; }
        public int CompanyId { get; set; }
        public IEnumerable<UserViewModel> Employees { get; set; }
        public int EmployeesCount { get; set; }
        public int OnlineEmployeesCount { get; set; }
    }

}