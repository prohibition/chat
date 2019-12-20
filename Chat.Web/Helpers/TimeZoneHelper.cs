using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Chat.Web.Helpers
{
    public static class TimeZoneHelper
    {
        public static DateTime ApplyTimeZone(this DateTime datetime,string timezone)
        {
           
            try
            {
                var timezoneId = getTimeZoneId(timezone);

                if (timezoneId != "")
                { 
                    DateTime utcTime = datetime.ToUniversalTime();
                    TimeZoneInfo timeInfo = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
                    DateTime converteddatetime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, timeInfo);

                    return converteddatetime;
                }
                return datetime;
            }
            catch (TimeZoneNotFoundException)
            {
                return datetime;
            }
            catch (InvalidTimeZoneException)
            {
                return datetime;
            }
        }

        public static string getTimeZoneId(string timezone)
        {

            /*
            Pacific/Honolulu" code="HST" utctime="UTC-10">HAWAII
            America/Anchorage" code="AKDT" utctime="UTC-8">ALASKA
            America/Los_Angeles" code="PDT" utctime="UTC-7">PACIFIC
            America/Edmonton" code="MDT" utctime="UTC-6">MOUNTAIN
            America/Chicago" code="CDT" utctime="UTC-5">CENTRAL
            America/New_York" code="EDT" utctime="UTC-4">EASTERN

            UTC-11: Samoa Standard Time (ST)
            UTC-10: Hawaii-Aleutian Standard Time (HAT)
            UTC-9: Alaska Standard Time (AKT)
            UTC−8: Pacific Standard Time (PT)
            UTC−7: Mountain Standard Time (MT)
            UTC−6: Central Standard Time (CT)
            UTC−5: Eastern Standard Time (ET)
            UTC−4: Atlantic Standard Time (AST)
            UTC+10: Chamorro Standard Time (ChT)
            UTC+12: Wake Island Time Zone (WIT)
            */
            var timezoneId = "";

            timezone = (timezone == null) ? "": timezone;

            switch (timezone.Trim())
            {
                case "Pacific/Honolulu":
                    timezoneId = "Hawaii-Aleutian Standard Time";
                    break;
                case "America/Anchorages":
                    timezoneId = "Alaska Standard Time";
                    break;
                case "America/Los_Angeles":
                    timezoneId = "Pacific Standard Times";
                    break;
                case "America/Edmonton":
                    timezoneId = "Mountain Standard Time";
                    break;
                case "America/Chicago":
                    timezoneId = "Central Standard Time";
                    break;
                case "America/New_York":
                    timezoneId = "Eastern Standard Time";
                    break;
            }

            return timezoneId;
        }
    }
}