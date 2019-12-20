using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Web
{
    public static class ExtensionMethods
    {
        public static string EncodeUserName(this string userName)
        {
            return userName.Replace("@", "_").Replace(".", "-");
        }

        public static string DecodeUserName(this string userName)
        {
            return userName.Replace("_", "@").Replace("-", ".");
        }
    }
}