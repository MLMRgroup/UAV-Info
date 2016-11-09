using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;

namespace UAV_Info
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<FlightBean> flightBeanList;

        // key: 飞行的某时刻, value: 数据在flightBeanList中的索引
        private Dictionary<string, int> indexDict;

        public MainWindow()
        {
            InitializeComponent();
            flightBeanList = new List<FlightBean>();
            indexDict = new Dictionary<string, int>();
        }

        private void importAngleData(object sender, RoutedEventArgs args) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true) {
                string fileName = openFileDialog.FileName;
                using(FileStream fs = File.Open(fileName, FileMode.Open)){
                    StreamReader sr = new StreamReader(fs);
                    string s;
                    while ((s = sr.ReadLine()) != null)
                    {
                        string[] split = s.Split(',');
                        if (indexDict.ContainsKey(split[1]))
                        {
                            FlightBean fb = flightBeanList[indexDict[split[1]]];
                            fb.pitch = Convert.ToDouble(split[2]);
                            fb.yaw = Convert.ToDouble(split[4]);
                            fb.roll = Convert.ToDouble(split[3]);
                        }
                        else
                        {
                            FlightBean fb = new FlightBean();
                            fb.time = split[1];
                            fb.pitch = Convert.ToDouble(split[2]);
                            fb.yaw = Convert.ToDouble(split[4]);
                            fb.roll = Convert.ToDouble(split[3]);
                            int index = flightBeanList.Count;
                            flightBeanList.Add(fb);
                            indexDict.Add(split[1], index);
                        }
                    }
                }
                // 绘图

            }
        }

        private void importTraceData(object sender, RoutedEventArgs args)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {

            }
        }
    }
}
