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
    //gpx_trans类，用来读GPX文件
    class gpx_trans
    {
        //存时间和对应轨迹数据对象，为提高效率，存在字典和列表对象中
        public Dictionary<string,int> gpxdata=new Dictionary<string,int>();
        public List <FlightBean> gpxlist=new List<FlightBean>();
        //打开的文件路径
        private string strOpenFileName;
        //子线程
        public Thread thread1;

        public gpx_trans()
        {
        }

        public gpx_trans(Dictionary<string,int> a,List<FlightBean> b)
        {
            //给成员变量赋初值
            gpxdata = a;
            gpxlist = b;
        }

        //开启线程
        public void start(string sourceFile)
        {
            strOpenFileName = sourceFile;
            thread1 = null;
            thread1 = new Thread(th1);
            //后台线程
            thread1.IsBackground = true;
            thread1.Name = "hxb";
            //线程的优先级设为最高
            thread1.Priority = ThreadPriority.Highest;
            //开始子线程
            thread1.Start();
        }

        private void th1()
        {
            //线程用来运行GarminRead函数
            GarminRead(strOpenFileName);
        }
 
        public void GarminRead(string sourceFile)
        {
            //读Xml文本的对象
            XmlTextReader reader = null;
            XmlReader r1 = null;
            string wd="", jd="";
            string djd="", dwd="",ele="",name="",time="";

            try
            {
                //初始化reader
                reader = new XmlTextReader(sourceFile);
                //设置处理空白的方式
                reader.WhitespaceHandling = WhitespaceHandling.None;

                while (reader.Read())
                {
                    //如果读到的路点（waypoint）信息
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "wpt")
                    {
                        //在此读取兴趣点
                        while (reader.MoveToNextAttribute())
                        {
                            //如果读到的为经纬度
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
                            //读到的为时间，元素或者名称
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
                    //如果读到的为轨迹（track）信息
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
                            //如果读到了一个轨迹点（track point）
                            if (r1.NodeType == XmlNodeType.Element && reader.Name == "trkpt")
                            {
                                while (r1.MoveToNextAttribute())
                                {
                                    //如果读到经纬度
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
                                        //读到时间
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
                                //转化成格式化时间
                                string format_time = TimeUtils.toformatTime(time);
                                //如果数据中已包含此时间点，则覆盖原来的数据
                                if (gpxdata.ContainsKey(format_time))
                                {
                                    FlightBean fb = gpxlist[gpxdata[format_time]];
                                    fb.time = format_time;
                                    fb.lat = Convert.ToDouble(wd);
                                    fb.lng = Convert.ToDouble(jd);
                                }
                                //数据不包含此时间点，则新建FlightBean对象，进行赋值，并加到字典和列表中
                                else
                                {
                                    //初始化经纬度为0，表示该时间点未读到经纬度数据
                                    FlightBean fb = new FlightBean("", FlightBean.NoneLat, FlightBean.NoneLng, FlightBean.NoneAngle, FlightBean.NoneAngle, FlightBean.NoneAngle);
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
                //释放reader的资源
                if (reader != null)
                    reader.Close();
            }
        }
    }
}
