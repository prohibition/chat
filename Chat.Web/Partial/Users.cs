using Chat.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Web.TCModel
{
    public partial class User
    {
        public string DisplayName => $"{FirstName} {InitialName} {LastName}";
        public string CompanyName { get; set; }
        public string Avatar => ProfilePicture;
    }
}