using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Threading;
using System.Collections;//��̬����
namespace UAV_Info
{
    class gpx_trans
    {
        public Dictionary<string,int> gpxdata=new Dictionary<string,int>();//��ʱ��Ͷ�Ӧ�켣���ݶ���
        public List <FlightBean> gpxlist=new List<FlightBean>();

        private string strOpenFileName;//�򿪵��ļ�·��
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
            thread1.IsBackground = true;//��̨�߳�
            thread1.Name = "hxb";
            thread1.Priority = ThreadPriority.Highest;
            thread1.Start();//��ʼ���߳�
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
                        //�ڴ˶�ȡ��Ȥ��
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
                    //�켣
                    else if (reader.NodeType == XmlNodeType.Element && reader.Name == "trk")
                    {
                        //----------//һ����       
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
