using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_Info
{
    /// <summary>
    /// DateTime对象和时间字符串之间的转换工具类
    /// </summary>
    class TimeUtils
    {

        /// <summary>
        /// 标准时间格式：yyyy-MM-dd hh:mm:ss，转换为DateTime对象
        /// <param name="time">时间字符串yyyy-MM-dd hh:mm:ss</param>
        /// </summary> 
        public static DateTime strToDateTime(string time) {
            return Convert.ToDateTime(time);
        }

        /// <summary>
        /// 对两种类型的文件中的时间格式进行标准化
        /// <param name="time">未格式化的时间字符串</param>
        /// <returns>标准化后的时间字符串</returns>
        /// </summary>
        /// <example>
        /// gpx文件中时间格式2016-05-29T16:36:09Z，GYT文件中的时间格式160529174133, 统一化为yyyy-MM-dd hh:mm:ss
        /// </example>
        ///
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

        /// <summary>
        /// 将DateTime对象转换为时间字符串，转换后的格式为：yyyy-MM-dd hh:mm:ss
        /// <param name="time">DateTime对象</param>
        /// <returns>转换后的格式为：yyyy-MM-dd hh:mm:ss</returns>
        /// </summary>
        ///
        public static string DateTimeToStr(DateTime dt) {
            return dt.ToString("s").Replace('T', ' ');
        }
    }
}
