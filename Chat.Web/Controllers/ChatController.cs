using Chat.Web.ChatModel;
using Chat.Web.Helpers;
using Chat.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Chat.Web.Controllers
{
    
    public class ChatController : Controller
    {
    
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public JsonResult UnRead(string T)
        {
            if (T != null && T.Length > 0)
            {
                var user = TitleConnectHelper.GetUserbyToken(T);

                if (user != null)
                {
                    var userId= user.CompanyEmail.EncodeUserName();

                    var messages = new MessageCountViewModel()
                    {
                        UserCompanyEmail = userId.DecodeUserName(),
                        Error = ""
                    };

                    using (var context = new ChatWebEntities())
                    {
                        var messageslist=context.PrivateMessages.Where(m => m.ToId == userId && !m.ReadStatus)
                           .GroupBy(p => p.FromId)
                           .Select(g => new { Count = g.Count() }).ToList();

                        messageslist.ForEach(x =>
                        {
                            messages.MessageCount += x.Count;   
                        });
                    }

                    return Json(messages, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new MessageCountViewModel() { Error="Invalid User",MessageCount=0,UserCompanyEmail=string.Empty }, JsonRequestBehavior.AllowGet);
        }
    }
}