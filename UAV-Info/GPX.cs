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
    //gpx_trans�࣬������GPX�ļ�
    class gpx_trans
    {
        //��ʱ��Ͷ�Ӧ�켣���ݶ���Ϊ���Ч�ʣ������ֵ���б������
        public Dictionary<string,int> gpxdata=new Dictionary<string,int>();
        public List <FlightBean> gpxlist=new List<FlightBean>();
        //�򿪵��ļ�·��
        private string strOpenFileName;
        //���߳�
        public Thread thread1;

        public gpx_trans()
        {
        }

        public gpx_trans(Dictionary<string,int> a,List<FlightBean> b)
        {
            //����Ա��������ֵ
            gpxdata = a;
            gpxlist = b;
        }

        //�����߳�
        public void start(string sourceFile)
        {
            strOpenFileName = sourceFile;
            thread1 = null;
            thread1 = new Thread(th1);
            //��̨�߳�
            thread1.IsBackground = true;
            thread1.Name = "hxb";
            //�̵߳����ȼ���Ϊ���
            thread1.Priority = ThreadPriority.Highest;
            //��ʼ���߳�
            thread1.Start();
        }

        private void th1()
        {
            //�߳���������GarminRead����
            GarminRead(strOpenFileName);
        }
 
        public void GarminRead(string sourceFile)
        {
            //��Xml�ı��Ķ���
            XmlTextReader reader = null;
            XmlReader r1 = null;
            string wd="", jd="";
            string djd="", dwd="",ele="",name="",time="";

            try
            {
                //��ʼ��reader
                reader = new XmlTextReader(sourceFile);
                //���ô���հ׵ķ�ʽ
                reader.WhitespaceHandling = WhitespaceHandling.None;

                while (reader.Read())
                {
                    //���������·�㣨waypoint����Ϣ
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "wpt")
                    {
                        //�ڴ˶�ȡ��Ȥ��
                        while (reader.MoveToNextAttribute())
                        {
                            //���������Ϊ��γ��
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
                            //������Ϊʱ�䣬Ԫ�ػ�������
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
                    //���������Ϊ�켣��track����Ϣ
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
                            //���������һ���켣�㣨track point��
                            if (r1.NodeType == XmlNodeType.Element && reader.Name == "trkpt")
                            {
                                while (r1.MoveToNextAttribute())
                                {
                                    //���������γ��
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
                                        //����ʱ��
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
                                //ת���ɸ�ʽ��ʱ��
                                string format_time = TimeUtils.toformatTime(time);
                                //����������Ѱ�����ʱ��㣬�򸲸�ԭ��������
                                if (gpxdata.ContainsKey(format_time))
                                {
                                    FlightBean fb = gpxlist[gpxdata[format_time]];
                                    fb.time = format_time;
                                    fb.lat = Convert.ToDouble(wd);
                                    fb.lng = Convert.ToDouble(jd);
                                }
                                //���ݲ�������ʱ��㣬���½�FlightBean���󣬽��и�ֵ�����ӵ��ֵ���б���
                                else
                                {
                                    //��ʼ����γ��Ϊ0����ʾ��ʱ���δ������γ������
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
                //�ͷ�reader����Դ
                if (reader != null)
                    reader.Close();
            }
        }
    }
}
