using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_Info
{
    class TimeUtils
    {
        public static DateTime strToDateTime(string time) {
            return Convert.ToDateTime(time);
        }

        public static string toformatTime(string time) {
            string formatTime;
            if (time.Contains('T'))
            {
                formatTime = time.Replace('T', ' ').Substring(0, time.Length - 1);
            }
            else {
                formatTime = "20" + time.Substring(0, 2) + "-" + time.Substring(2, 2) + "-" + time.Substring(4, 2) + " " + time.Substring(6, 2) + ":" + time.Substring(8, 2) + ":" + time.Substring(10, 2);
            }
            return formatTime;
        }

        public static string DateTimeToStr(DateTime dt) {
            return dt.ToString("s").Replace('T', ' ');
        }
    }
}
