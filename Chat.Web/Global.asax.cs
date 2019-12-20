using AutoMapper;
using Chat.Web.Mappings;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Chat.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<GroupMessageProfile>();
                cfg.AddProfile<PrivateMessageProfile>();
                cfg.AddProfile<RoomProfile>();
                cfg.AddProfile<UserProfile>();
            });


            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(110);

            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(30);

            GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(10);

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

        }
    }
}
