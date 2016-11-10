using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Threading;
using System.Collections;//动态数组
namespace UAV_Info
{
    class gpx_trans
    {
        public Dictionary<string,int> gpxdata=new Dictionary<string,int>();//存时间和对应轨迹数据对象
        public List <FlightBean> gpxlist=new List<FlightBean>();

        private string strOpenFileName;//打开的文件路径
        public Thread thread1;

        public gpx_trans()
        {
        }

        public gpx_trans(Dictionary<string,int> a,List<FlightBean> b)
        {
            gpxdata = a;
            gpxlist = b;
        }

        public void start(string sourceFile)
        {
            strOpenFileName = sourceFile;
            thread1 = null;
            thread1 = new Thread(th1);
            thread1.IsBackground = true;//后台线程
            thread1.Name = "hxb";
            thread1.Priority = ThreadPriority.Highest;
            thread1.Start();//开始子线程
        }

        private void th1()
        {
            GarminRead(strOpenFileName);
        }
 
        public void GarminRead(string sourceFile)
        {
            XmlTextReader reader = null;
            XmlReader r1 = null;
            string wd="", jd="";
            string djd="", dwd="",ele="",name="",time="";

            try
            {
                reader = new XmlTextReader(sourceFile);
                reader.WhitespaceHandling = WhitespaceHandling.None;

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "wpt")
                    {
                        //在此读取兴趣点
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "lat")
                            {
                                 dwd= reader.Value;
                            }
                            else if (reader.Name == "lon")
                            {
                                  djd=reader.Value;
                            }
                        }
                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element)
                            {
                                if (reader.Name == "time")
                                {
                                    reader.Read();
                                    time = reader.Value;
                                }
                                if (reader.Name == "ele")
                                {
                                    reader.Read();
                                    ele = reader.Value;
                                }
                                if (reader.Name == "name")
                                {
                                    reader.Read();
                                    name = reader.Value;
                                    break;
                                }
                            }
                        }
                    }
                    //轨迹
                    else if (reader.NodeType == XmlNodeType.Element && reader.Name == "trk")
                    {
                        //----------//一条轨       
                        r1 = reader.ReadSubtree();
                        while (r1.Read())
                        {
                            if (r1.NodeType == XmlNodeType.Element && r1.Name == "name")
                            {
                                r1.Read();
                                break;
                            }
                        }
                        while (r1.Read())
                        {
                            if (  r1.NodeType == XmlNodeType.Element && reader.Name == "trkpt")
                            {
                                while (r1.MoveToNextAttribute())
                                {
                                    if (r1.Name == "lat")
                                    {
                                        wd = r1.Value;
                                    }
                                    else if (r1.Name == "lon")
                                    {
                                        jd = r1.Value;
                                    }
                                }
                                while (reader.Read())
                                {
                                    if (reader.NodeType == XmlNodeType.Element)
                                    {
                                        if (reader.Name == "time")
                                        {
                                            reader.Read();
                                            time = reader.Value;
                                            break;
                                        }
                                        if (reader.Name == "ele")
                                        {
                                            reader.Read();
                                            ele = reader.Value;
                                        }
                                    }
                                }
                                string format_time = TimeUtils.toformatTime(time);
                                if (gpxdata.ContainsKey(format_time))
                                {
                                    FlightBean fb = gpxlist[gpxdata[format_time]];
                                    fb.time = format_time;
                                    fb.lat = Convert.ToDouble(wd);
                                    fb.lng = Convert.ToDouble(jd);
                                }
                                else
                                {
                                    FlightBean fb = new FlightBean("", 0, 0, 0, 0, 0);
                                    fb.time = format_time;
                                    fb.lat = Convert.ToDouble(wd);
                                    fb.lng = Convert.ToDouble(jd);
                                    int index = gpxlist.Count;
                                    gpxlist.Add(fb);
                                    gpxdata.Add(format_time, index);
                                }
                            }
                        }
                    }
                }//while
//-------------------
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
    }
}
