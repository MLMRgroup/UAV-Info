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
using Microsoft.Research.DynamicDataDisplay;
using System.IO;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Windows.Threading;
using System.Threading;

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

        private List<FlightBean> normalizedFlightBeanList;


        public MainWindow()
        {
            InitializeComponent();
            flightBeanList = new List<FlightBean>();
            indexDict = new Dictionary<string, int>();
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        Span normalizeSpan = new Span();
        Span timeSpan = new Span();

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
          /*plotPitch.Viewport.SetBinding(Viewport2D.VisibleProperty,
                    new Binding("Visible") { Source = plotRoll.Viewport, Mode = BindingMode.TwoWay });
            plotRoll.Viewport.SetBinding(Viewport2D.VisibleProperty,
                    new Binding("Visible") { Source = plotYaw.Viewport, Mode = BindingMode.TwoWay });
           */
          //plotPitch.Children.Remove(plotPitch.MouseNavigation);

          //为plotter添加基准线组件
          plotPitch.Children.Add(normalizeSpan.LineA);
          plotPitch.Children.Add(normalizeSpan.LineB);
          plotYaw.Children.Add(normalizeSpan.LineA);
          plotYaw.Children.Add(normalizeSpan.LineB);
          plotRoll.Children.Add(normalizeSpan.LineA);
          plotRoll.Children.Add(normalizeSpan.LineB);

          plotPitchNormal.Children.Add(timeSpan.LineA);
          plotPitchNormal.Children.Add(timeSpan.LineB);
          plotYawNormal.Children.Add(timeSpan.LineA);
          plotYawNormal.Children.Add(timeSpan.LineB);
          plotRollNormal.Children.Add(timeSpan.LineA);
          plotRollNormal.Children.Add(timeSpan.LineB);

          //plotPitch.Viewport.Restrictions.Add();
          // plotYaw.DefaultContextMenu.Remove();
          //删去双击放大事件
          plotPitch.Children.Remove(plotPitch.KeyboardNavigation);
          plotYaw.Children.Remove(plotYaw.KeyboardNavigation);
          plotRoll.Children.Remove(plotRoll.KeyboardNavigation);
          plotPitchNormal.Children.Remove(plotPitchNormal.KeyboardNavigation);
          plotYawNormal.Children.Remove(plotYawNormal.KeyboardNavigation);
          plotRollNormal.Children.Remove(plotRollNormal.KeyboardNavigation);
          plotTrace.Children.Remove(plotRollNormal.KeyboardNavigation);


          //双击描线事件
          plotPitch.MouseDoubleClick += onDoubleCkick_AngleChart;
          plotYaw.MouseDoubleClick += onDoubleCkick_AngleChart;
          plotRoll.MouseDoubleClick += onDoubleCkick_AngleChart;

          plotPitchNormal.MouseDoubleClick += onDoubleCkick_AngleChart;
          plotYawNormal.MouseDoubleClick += onDoubleCkick_AngleChart;
          plotRollNormal.MouseDoubleClick += onDoubleCkick_AngleChart;

          // Add handler
          plotPitch.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(Viewport_PropertyChanged);
          plotYaw.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(Viewport_PropertyChanged);
          plotRoll.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(Viewport_PropertyChanged);

          plotPitchNormal.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(Viewport_PropertyChanged);
          plotYawNormal.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(Viewport_PropertyChanged);
          plotRollNormal.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(Viewport_PropertyChanged);
        }

        private void OnClick_Reset(object sender, RoutedEventArgs e)
        {
            if(sender.Equals(btnNormSpanReset))
            {
                normalizeSpan.Reset();
                btnNormlize.IsEnabled = false;
            }
            else if(sender.Equals(btnTimeSpanReset))
            {
                timeSpan.Reset();
            }
        }
        // Respond to changes
        void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
        {
            /*if (e.PropertyName == "Visible")
            {
                plotPitch.Viewport.Visible = new Rect(((Viewport2D)sender).Visible.X, minPitchIndex, ((Viewport2D)sender).Visible.Width, maxPitchIndex);
                plotYaw.Viewport.Visible = new Rect(((Viewport2D)sender).Visible.X, minYawIndex, ((Viewport2D)sender).Visible.Width, maxYawIndex);
                plotRoll.Viewport.Visible = new Rect(((Viewport2D)sender).Visible.X, minRollIndex, ((Viewport2D)sender).Visible.Width, maxRollIndex);
            }
            //依据事件的来源，同步某侧的三个chart
            if (sender.Equals(plotPitch.Viewport) || sender.Equals(plotYaw.Viewport) || sender.Equals(plotRoll.Viewport))
            {
                plotPitch.Viewport.Visible = new Rect(((Viewport2D)sender).Visible.X, plotPitch.Viewport.Visible.Y, ((Viewport2D)sender).Visible.Width, plotPitch.Viewport.Visible.Height);
                plotYaw.Viewport.Visible = new Rect(((Viewport2D)sender).Visible.X, plotYaw.Viewport.Visible.Y, ((Viewport2D)sender).Visible.Width, plotYaw.Viewport.Visible.Height);
                plotRoll.Viewport.Visible = new Rect(((Viewport2D)sender).Visible.X, plotRoll.Viewport.Visible.Y, ((Viewport2D)sender).Visible.Width, plotRoll.Viewport.Visible.Height);
            }
            else if (sender.Equals(plotPitchNormal.Viewport) || sender.Equals(plotYawNormal.Viewport) || sender.Equals(plotRollNormal.Viewport))
            {
                plotPitchNormal.Viewport.Visible = new Rect(((Viewport2D)sender).Visible.X, plotPitchNormal.Viewport.Visible.Y, ((Viewport2D)sender).Visible.Width, plotPitchNormal.Viewport.Visible.Height);
                plotYawNormal.Viewport.Visible = new Rect(((Viewport2D)sender).Visible.X, plotYawNormal.Viewport.Visible.Y, ((Viewport2D)sender).Visible.Width, plotYawNormal.Viewport.Visible.Height);
                plotRollNormal.Viewport.Visible = new Rect(((Viewport2D)sender).Visible.X, plotRollNormal.Viewport.Visible.Y, ((Viewport2D)sender).Visible.Width, plotRollNormal.Viewport.Visible.Height);
            }*/
        }

        private void onDoubleCkick_AngleChart(object sender, MouseEventArgs e)
        {
            ChartPlotter plotter = (ChartPlotter)sender;
            //坐标转换
            var transform = plotter.Transform;
            var mouseScreenPosition = Mouse.GetPosition(plotter.CentralGrid);
            var mousePositionInData = mouseScreenPosition.ScreenToViewport(transform);

            if (sender.Equals(plotPitch) || sender.Equals(plotYaw) || sender.Equals(plotRoll))
            {
                normalizeSpan.AddLine(mousePositionInData.X);
            }
            else if (sender.Equals(plotPitchNormal) || sender.Equals(plotYawNormal) || sender.Equals(plotRollNormal))
            {
                timeSpan.AddLine(mousePositionInData.X);
            }
            if (normalizeSpan.IsSet) {
                btnNormlize.IsEnabled = true;
            }
            if (timeSpan.IsSet) {
                analyseAngleNormalized();
            }
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
                        string time = TimeUtils.toformatTime(split[1]);
                        if (indexDict.ContainsKey(time))
                        {
                            FlightBean fb = flightBeanList[indexDict[time]];
                            fb.pitch = Convert.ToDouble(split[2]);
                            fb.yaw = Convert.ToDouble(split[4]);
                            fb.roll = Convert.ToDouble(split[3]);
                        }
                        else
                        {
                            FlightBean fb = new FlightBean("",0,0,0,0,0);
                            fb.time = time;
                            fb.pitch = Convert.ToDouble(split[2]);
                            fb.yaw = Convert.ToDouble(split[4]);
                            fb.roll = Convert.ToDouble(split[3]);
                            int index = flightBeanList.Count;
                            flightBeanList.Add(fb);
                            indexDict.Add(time, index);
                        }
                    }
                }
                indexDict = (from entry in indexDict orderby entry.Key ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
                // 绘图
                plotAngle("pitch");
                plotAngle("yaw");
                plotAngle("roll");
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
                // 轨迹数据读入
                string fileName = openFileDialog.FileName;
                gpx_trans gpx = new gpx_trans(indexDict,flightBeanList);
                gpx.start(fileName);
                while (gpx.thread1.ThreadState != System.Threading.ThreadState.Stopped) { }

                indexDict = gpx.gpxdata;
                flightBeanList = gpx.gpxlist;
                indexDict = (from entry in indexDict orderby entry.Key ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
                PlotTrace();
            }
        }
        private void PlotTrace()
        {
            List<double> latList = new List<double>();
            List<double> lngList = new List<double>();
            foreach (string key in indexDict.Keys)
            {
                if (flightBeanList[indexDict[key]].lat != 0 && flightBeanList[indexDict[key]].lng != 0)
                {
                    latList.Add(flightBeanList[indexDict[key]].lat);
                    lngList.Add(flightBeanList[indexDict[key]].lng);
                }
            }
            EnumerableDataSource<double> latDataSource = new EnumerableDataSource<double>(latList);
            latDataSource.SetXMapping(y => y);
            EnumerableDataSource<double> logDataSource = new EnumerableDataSource<double>(lngList);
            logDataSource.SetYMapping(x => x);
            CompositeDataSource compositeDataSource = new CompositeDataSource(logDataSource, latDataSource);
            LineGraph lineG = new LineGraph();
            lineG.Description = new PenDescription("轨迹");
            lineG.DataSource = compositeDataSource;
            plotTrace.Children.RemoveAll(lineG.GetType());
            plotTrace.Viewport.FitToView();
            plotTrace.Children.Add(lineG);

        }

        private void plotAngle(string whichAngle) {
            // 例子，绘制俯仰角
            List<DateTime> dateTimeList = new List<DateTime>();
            List<double> angleList = new List<double>();
            if (whichAngle == "pitch") {
                foreach (string key in indexDict.Keys)
                {
                    dateTimeList.Add(TimeUtils.strToDateTime(key));
                    angleList.Add(flightBeanList[indexDict[key]].pitch);
                }

                EnumerableDataSource<DateTime> datesDataSource = new EnumerableDataSource<DateTime>(dateTimeList);
                datesDataSource.SetXMapping(x => dateAxis_angle.ConvertToDouble(x));
                EnumerableDataSource<double> angleDataSource = new EnumerableDataSource<double>(angleList);
                angleDataSource.SetYMapping(y => y);
                CompositeDataSource compositeDataSource = new CompositeDataSource(datesDataSource, angleDataSource);
                LineGraph lineG = new LineGraph();
                lineG.Description = new PenDescription("俯仰");
                lineG.DataSource = compositeDataSource;
                plotPitch.Children.RemoveAll(lineG.GetType());
                plotPitch.Viewport.FitToView();
                plotPitch.Children.Add(lineG);
            } else if (whichAngle == "yaw") {
                foreach (string key in indexDict.Keys)
                {
                    dateTimeList.Add(TimeUtils.strToDateTime(key));
                    angleList.Add(flightBeanList[indexDict[key]].yaw);
                }

                EnumerableDataSource<DateTime> datesDataSource = new EnumerableDataSource<DateTime>(dateTimeList);
                datesDataSource.SetXMapping(x => dateAxis_angle.ConvertToDouble(x));
                EnumerableDataSource<double> angleDataSource = new EnumerableDataSource<double>(angleList);
                angleDataSource.SetYMapping(y => y);
                CompositeDataSource compositeDataSource = new CompositeDataSource(datesDataSource, angleDataSource);
                LineGraph lineG = new LineGraph();
                lineG.Description = new PenDescription("偏航");
                lineG.DataSource = compositeDataSource;
                plotYaw.Children.RemoveAll(lineG.GetType());
                plotYaw.Viewport.FitToView();
                plotYaw.Children.Add(lineG);
            }
            else {
                foreach (string key in indexDict.Keys)
                {
                    dateTimeList.Add(TimeUtils.strToDateTime(key));
                    angleList.Add(flightBeanList[indexDict[key]].roll);
                }

                EnumerableDataSource<DateTime> datesDataSource = new EnumerableDataSource<DateTime>(dateTimeList);
                datesDataSource.SetXMapping(x => dateAxis_angle.ConvertToDouble(x));
                EnumerableDataSource<double> angleDataSource = new EnumerableDataSource<double>(angleList);
                angleDataSource.SetYMapping(y => y);
                CompositeDataSource compositeDataSource = new CompositeDataSource(datesDataSource, angleDataSource);
                LineGraph lineG = new LineGraph();
                lineG.Description = new PenDescription("滚转");
                lineG.DataSource = compositeDataSource;
                plotRoll.Children.RemoveAll(lineG.GetType());
                plotRoll.Viewport.FitToView();
                plotRoll.Children.Add(lineG);
            }
            
        }

        private void plotTRace(double timeA = 0, double timeB = 0)
        {
            List<double> latList = new List<double>();
            List<double> logList = new List<double>();
            foreach (string key in indexDict.Keys)
            {
                if (flightBeanList[indexDict[key]].lat!=0 && flightBeanList[indexDict[key]].lng!=0)
                {
                    latList.Add(flightBeanList[indexDict[key]].lat);
                    logList.Add(flightBeanList[indexDict[key]].lng);
                }
            }

            EnumerableDataSource<double> latDataSource = new EnumerableDataSource<double>(latList);
            latDataSource.SetXMapping(y => y);
            EnumerableDataSource<double> logDataSource = new EnumerableDataSource<double>(logList);
            logDataSource.SetYMapping(x => x);
            CompositeDataSource compositeDataSource = new CompositeDataSource(logDataSource, latDataSource);
            LineGraph lineG = new LineGraph();
            lineG.Description = new PenDescription("轨迹");
            lineG.DataSource = compositeDataSource;
            plotTrace.Children.RemoveAll(lineG.GetType());
            plotTrace.Viewport.FitToView();
            plotTrace.Children.Add(lineG);
        }

        private void plotNormalizedAngle(string whichAngle)
        {
            // 例子，绘制俯仰角
            List<DateTime> dateTimeList = new List<DateTime>();
            List<double> angleList = new List<double>();
            if (whichAngle == "pitch")
            {
                foreach (string key in indexDict.Keys)
                {
                    dateTimeList.Add(TimeUtils.strToDateTime(key));
                    angleList.Add(normalizedFlightBeanList[indexDict[key]].pitch);
                }

                EnumerableDataSource<DateTime> datesDataSource = new EnumerableDataSource<DateTime>(dateTimeList);
                datesDataSource.SetXMapping(x => dateAxis_angleNormal.ConvertToDouble(x));
                EnumerableDataSource<double> angleDataSource = new EnumerableDataSource<double>(angleList);
                angleDataSource.SetYMapping(y => y);
                CompositeDataSource compositeDataSource = new CompositeDataSource(datesDataSource, angleDataSource);
                LineGraph lineG = new LineGraph();
                lineG.Description = new PenDescription("俯仰");
                lineG.DataSource = compositeDataSource;
                plotPitchNormal.Children.RemoveAll(lineG.GetType());
                plotPitchNormal.Viewport.FitToView();
                plotPitchNormal.Children.Add(lineG);
            }
            else if (whichAngle == "yaw")
            {
                foreach (string key in indexDict.Keys)
                {
                    dateTimeList.Add(TimeUtils.strToDateTime(key));
                    angleList.Add(normalizedFlightBeanList[indexDict[key]].yaw);
                }

                EnumerableDataSource<DateTime> datesDataSource = new EnumerableDataSource<DateTime>(dateTimeList);
                datesDataSource.SetXMapping(x => dateAxis_angleNormal.ConvertToDouble(x));
                EnumerableDataSource<double> angleDataSource = new EnumerableDataSource<double>(angleList);
                angleDataSource.SetYMapping(y => y);
                CompositeDataSource compositeDataSource = new CompositeDataSource(datesDataSource, angleDataSource);
                LineGraph lineG = new LineGraph();
                lineG.Description = new PenDescription("偏航");
                lineG.DataSource = compositeDataSource;
                plotYawNormal.Children.RemoveAll(lineG.GetType());
                plotYawNormal.Viewport.FitToView();
                plotYawNormal.Children.Add(lineG);
            }
            else
            {
                foreach (string key in indexDict.Keys)
                {
                    dateTimeList.Add(TimeUtils.strToDateTime(key));
                    angleList.Add(normalizedFlightBeanList[indexDict[key]].roll);
                }

                EnumerableDataSource<DateTime> datesDataSource = new EnumerableDataSource<DateTime>(dateTimeList);
                datesDataSource.SetXMapping(x => dateAxis_angleNormal.ConvertToDouble(x));
                EnumerableDataSource<double> angleDataSource = new EnumerableDataSource<double>(angleList);
                angleDataSource.SetYMapping(y => y);
                CompositeDataSource compositeDataSource = new CompositeDataSource(datesDataSource, angleDataSource);
                LineGraph lineG = new LineGraph();
                lineG.Description = new PenDescription("滚转");
                lineG.DataSource = compositeDataSource;
                plotRollNormal.Children.RemoveAll(lineG.GetType());
                plotRollNormal.Viewport.FitToView();
                plotRollNormal.Children.Add(lineG);
            }

        }

        private void onClick_NormalizeAngle(object sender, EventArgs e) {
            DateTime timeStart = dateAxis_angle.ConvertFromDouble(normalizeSpan.valueOfLineA);
            string time1 = TimeUtils.DateTimeToStr(timeStart);
            List<FlightBean> list = new List<FlightBean>();
            DateTime timeEnd = dateAxis_angle.ConvertFromDouble(normalizeSpan.valueOfLineB);
            string time2 = TimeUtils.DateTimeToStr(timeEnd);

            if (false == indexDict.ContainsKey(time1))
            {
                time1 = indexDict.Keys.ElementAt(0);
            }
            if (false == indexDict.ContainsKey(time2))
            {
                time2 = indexDict.Keys.ElementAt(indexDict.Keys.Count - 1);
            }

            int index1 = indexDict[time1];
            int index2 = indexDict[time2];

            if (index1 < index2) {
                list = flightBeanList.GetRange(index1, index2 - index1);
            }
            else {
                list = flightBeanList.GetRange(index2, index1 - index2);
            }
            double meanOfPitch = (from l in list select l.pitch).Sum() / list.Count;
            double meanOfYaw = (from l in list select l.yaw).Sum() / list.Count;
            double meanOfRoll = (from l in list select l.roll).Sum() / list.Count;
            normalizedFlightBeanList = new List<FlightBean>(flightBeanList);
            foreach (FlightBean fb in normalizedFlightBeanList) {
                fb.pitch -= meanOfPitch;
                fb.yaw -= meanOfYaw;
                fb.roll -= meanOfRoll;
            }
            plotNormalizedAngle("pitch");
            plotNormalizedAngle("yaw");
            plotNormalizedAngle("roll");
        }

        private void analyseAngleNormalized() {
            //获取时间区间
            DateTime timeStart = dateAxis_angleNormal.ConvertFromDouble(timeSpan.valueOfLineA);
            string time1 = TimeUtils.DateTimeToStr(timeStart);
            DateTime timeEnd = dateAxis_angleNormal.ConvertFromDouble(timeSpan.valueOfLineB);
            string time2 = TimeUtils.DateTimeToStr(timeEnd);

            List<FlightBean> list = new List<FlightBean>();

            //超出时间范围，则校正为时间边界
            if (false == indexDict.ContainsKey(time1))
            {
                time1 = indexDict.Keys.ElementAt(0);
            }
            if (false == indexDict.ContainsKey(time2))
            {
                time2 = indexDict.Keys.ElementAt(indexDict.Keys.Count-1);
            }

            int index1 = indexDict[time1];
            int index2 = indexDict[time2];

            if (index1 < index2)
            {
                list = normalizedFlightBeanList.GetRange(index1, index2 - index1);
            }
            else
            {
                list = normalizedFlightBeanList.GetRange(index2, index1 - index2);
            }

            double maxOfPitch = (from l in list select l.pitch).Max();
            double minOfPitch = (from l in list select l.pitch).Min();
            double maxOfYaw = (from l in list select l.yaw).Max();
            double minOfYaw = (from l in list select l.yaw).Min();
            double maxOfRoll = (from l in list select l.roll).Max();
            double minOfRoll = (from l in list select l.roll).Min();
            pitchMaxTextBox.Text = maxOfPitch.ToString("f2");
            yawMaxTextBox.Text = maxOfYaw.ToString("f2");
            rollMaxTextBox.Text = maxOfRoll.ToString("f2");
            pitchMinTextBox.Text = minOfPitch.ToString("f2");
            yawMinTextBox.Text = minOfYaw.ToString("f2");
            rollMinTextBox.Text = minOfRoll.ToString("f2");

            //HighLight The Trace
            List<double> latListHLight = (from item in list where item.lat != 0 select item.lat).ToList();
            List<double> lngListHLight = (from item in list where item.lng != 0 select item.lng).ToList(); 
            
            EnumerableDataSource<double> latHLightDataSource = new EnumerableDataSource<double>(latListHLight);
            latHLightDataSource.SetXMapping(y => y);
            EnumerableDataSource<double> logHLightDataSource = new EnumerableDataSource<double>(lngListHLight);
            logHLightDataSource.SetYMapping(x => x);
            CompositeDataSource compositeHLightDataSource = new CompositeDataSource(logHLightDataSource, latHLightDataSource);
            LineGraph lineGHLight = new LineGraph
            {
                Stroke = Brushes.Green,
                StrokeThickness = 2,
            };
            lineGHLight.Description = new PenDescription("Highlight");
            lineGHLight.DataSource = compositeHLightDataSource;
            plotTrace.Viewport.FitToView();
            plotTrace.Children.Add(lineGHLight);

        }

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            Point pos = new Point(dateAxis_angle.ConvertToDouble(TimeUtils.strToDateTime(TimeUtils.toformatTime("160529163534"))), 0);
            Point zoomTo = pos.DataToScreen(plotPitch.Viewport.Transform);
            plotPitch.Viewport.Visible.Zoom(zoomTo, 3.0);
        }
    }
}
