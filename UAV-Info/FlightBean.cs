using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAV_Info
{
    class FlightBean
    {

        // 时刻
        private string _time;

        // 纬度
        private double _lat;

        // 经度
        private double _lng;

        // 俯仰角
        private double _pitch;

        // 偏航角
        private double _yaw;

        // 滚转角
        private double _roll;

        public FlightBean(string time, double lat, double lng, double pitch, double yaw, double roll)
        {
            _time = time;
            _lat = lat;
            _lng = lng;
            _pitch = pitch;
            _yaw = yaw;
            _roll = roll;
        }

        public string time { get { return _time; } set { _time = time; } }

        public double lat { get { return _lat; } set { _lat = lat; } }

        public double lng { get { return _lng; } set { _lng = lng; } }

        public double pitch { get { return _pitch; } set { _pitch = pitch; } }

        public double yaw { get { return _yaw; } set { _yaw = yaw; } }

        public double roll { get { return _roll; } set { _roll = roll; } }

        public DateTime timeToDateTime() {
            string formatTime = "20" + _time.Substring(0, 2) + "-" + _time.Substring(2, 2) + "-" + _time.Substring(4, 2) + " " + _time.Substring(6, 2) + ":" + _time.Substring(8, 2) + ":" + _time.Substring(10, 2);
            return Convert.ToDateTime(formatTime);
        }
    }
}
