﻿using System;
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

        public FlightBean() {

        }

        public FlightBean(string time, double lat, double lng, double pitch, double yaw, double roll)
        {
            _time = time;
            _lat = lat;
            _lng = lng;
            _pitch = pitch;
            _yaw = yaw;
            _roll = roll;
        }

        public string time { get { return _time; } set { _time = value; } }

        public double lat { get { return _lat; } set { _lat = value; } }

        public double lng { get { return _lng; } set { _lng = value; } }

        public double pitch { get { return _pitch; } set { _pitch = value; } }

        public double yaw { get { return _yaw; } set { _yaw = value; } }

        public double roll { get { return _roll; } set { _roll = value; } }

        public DateTime timeToDateTime() {
            return Convert.ToDateTime(_time);
        }
    }
}
