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
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Windows.Threading;

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
                        if (split.Length< 5) {
                            continue;
                        }
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
                plotAngle();
                DispatcherTimer animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                animationTimer.Tick += animationTimer_Tick;
                animationTimer.Start();
            }
        }

        private void importTraceData(object sender, RoutedEventArgs args)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {

            }
        }

        private void plotAngle() {
            // 例子，绘制俯仰角
            List<DateTime> dateTimeList = new List<DateTime>();
            List<double> angleList = new List<double>();
            foreach (string key in indexDict.Keys){
                dateTimeList.Add(toDataTime(key));
                angleList.Add(flightBeanList[indexDict[key]].pitch);
            }

            EnumerableDataSource<DateTime> datesDataSource = new EnumerableDataSource<DateTime>(dateTimeList);
            datesDataSource.SetXMapping(x => dateAxis.ConvertToDouble(x));
            EnumerableDataSource<double> angleDataSource = new EnumerableDataSource<double>(angleList);
            angleDataSource.SetYMapping(y => y);
            CompositeDataSource compositeDataSource = new CompositeDataSource(datesDataSource, angleDataSource);
            LineGraph lineG = new LineGraph();
            lineG.DataSource = compositeDataSource;
            angleChartPlotter.Children.Add(lineG);
        }

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            Point pos = new Point(dateAxis.ConvertToDouble(toDataTime("160529163534")), 0);
            Point zoomTo = pos.DataToScreen(angleChartPlotter.Viewport.Transform);
            angleChartPlotter.Viewport.Visible.Zoom(zoomTo, 3.0);
        }

        private DateTime toDataTime(string time) {
            string formatTime = "20" + time.Substring(0, 2) + "-" + time.Substring(2, 2) + "-" + time.Substring(4, 2) + " " + time.Substring(6, 2) + ":" + time.Substring(8, 2) + ":" + time.Substring(10, 2);
            return Convert.ToDateTime(formatTime);
        }
    }
}
