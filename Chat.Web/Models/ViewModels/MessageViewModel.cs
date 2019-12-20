using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Web.Models.ViewModels
{
    public class MessageViewModel
    {
        public string Content { get; set; }
        public string Timestamp { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string FromId { get; set; }
        public string ToId { get; set; }
        public string Avatar { get; set; }
        public int IsMine { get; set; }
    }

    public class UnReadMessageViewModel
    {
        public string FromId { get; set; }
        public string ToId { get; set; }
        public string CompanyId { get; set; }
        public int Count { get; set; }
    }

    public class MessageCountViewModel
    {
        public string UserCompanyEmail { get; set; }
        public int MessageCount { get; set; }
        public string Error { get; set; }
    }
}