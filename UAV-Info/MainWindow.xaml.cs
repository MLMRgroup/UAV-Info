﻿using System;
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
//using System.Windows.Forms;

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

        private bool isReopenAngleFile;
        public MainWindow()
        {
            InitializeComponent();
            //设置背景颜色
            SolidColorBrush NewColor = new SolidColorBrush();
            NewColor.Color = Color.FromArgb(255,110,204,210);
            this.Background = NewColor;

            flightBeanList = new List<FlightBean>();
            indexDict = new Dictionary<string, int>();
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            isReopenAngleFile = false;
        }

        //normalizeSpan：规范化选取区间  analyzeSpan：分析选取区间
        Span normalizeSpan = new Span();
        Span analyzeSpan = new Span();

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

          plotPitchNormal.Children.Add(analyzeSpan.LineA);
          plotPitchNormal.Children.Add(analyzeSpan.LineB);
          plotYawNormal.Children.Add(analyzeSpan.LineA);
          plotYawNormal.Children.Add(analyzeSpan.LineB);
          plotRollNormal.Children.Add(analyzeSpan.LineA);
          plotRollNormal.Children.Add(analyzeSpan.LineB);

          //plotPitch.Viewport.Restrictions.Add();
          //plotYaw.DefaultContextMenu.Remove();

          //删去双击放大事件
          plotPitch.Children.Remove(plotPitch.KeyboardNavigation);
          plotYaw.Children.Remove(plotYaw.KeyboardNavigation);
          plotRoll.Children.Remove(plotRoll.KeyboardNavigation);
          plotPitchNormal.Children.Remove(plotPitchNormal.KeyboardNavigation);
          plotYawNormal.Children.Remove(plotYawNormal.KeyboardNavigation);
          plotRollNormal.Children.Remove(plotRollNormal.KeyboardNavigation);
          traceChartPlotter.Children.Remove(traceChartPlotter.KeyboardNavigation);

          //添加双击描线事件
          plotPitch.MouseDoubleClick += onDoubleCkick_AngleChart;
          plotYaw.MouseDoubleClick += onDoubleCkick_AngleChart;
          plotRoll.MouseDoubleClick += onDoubleCkick_AngleChart;

          plotPitchNormal.MouseDoubleClick += onDoubleCkick_AngleChart;
          plotYawNormal.MouseDoubleClick += onDoubleCkick_AngleChart;
          plotRollNormal.MouseDoubleClick += onDoubleCkick_AngleChart;

          //添加坐标中的日期显示格式
          cordPitch.XTextMapping = x => dateAxisPitch.ConvertFromDouble(x).ToString("HH:mm:ss");
          cordYaw.XTextMapping = x => dateAxisYaw.ConvertFromDouble(x).ToString("HH:mm:ss");
          cordRoll.XTextMapping = x => dateAxisRoll.ConvertFromDouble(x).ToString("HH:mm:ss");
          cordPitchNormal.XTextMapping = x => dateAxisPitchNormal.ConvertFromDouble(x).ToString("HH:mm:ss");
          cordYawNormal.XTextMapping = x => dateAxisYawNormal.ConvertFromDouble(x).ToString("HH:mm:ss");
          cordRollNormal.XTextMapping = x => dateAxisRollNormal.ConvertFromDouble(x).ToString("HH:mm:ss");

          // Add handler
          /*
          plotPitch.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(Viewport_PropertyChanged);
          plotYaw.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(Viewport_PropertyChanged);
          plotRoll.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(Viewport_PropertyChanged);

          plotPitchNormal.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(Viewport_PropertyChanged);
          plotYawNormal.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(Viewport_PropertyChanged);
          plotRollNormal.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(Viewport_PropertyChanged);
          */
        }

        //重置按钮响应函数
        private void OnClick_Reset(object sender, RoutedEventArgs e)
        {
            //重置规范化区间
            if(sender.Equals(btnNormlizeSpanReset))
            {
                normalizeSpan.Reset();
                btnNormlize.IsEnabled = false;
            }
            //重置分析区间
            else if(sender.Equals(btnAnalyzeSpanReset))
            {
                analyzeSpan.Reset();
                clearanalysisTextBox();
                if (((LineGraph)traceChartPlotter.FindName("TraceHLight")) != null) { 
                    traceChartPlotter.Children.Remove((LineGraph)FindName("lineGHLight"));
                    traceChartPlotter.UnregisterName("lineGHLight");
                }
            }
        }
        // 窗口同步，暂时用不到
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
        //窗口双击时间
        private void onDoubleCkick_AngleChart(object sender, MouseEventArgs e)
        {
            ChartPlotter plotter = (ChartPlotter)sender;
            //坐标转换，得到当前坐标
            var transform = plotter.Transform;
            var mouseScreenPosition = Mouse.GetPosition(plotter.CentralGrid);
            var mousePositionInData = mouseScreenPosition.ScreenToViewport(transform);
            //添加基准线
            if (sender.Equals(plotPitch) || sender.Equals(plotYaw) || sender.Equals(plotRoll))
            {
                normalizeSpan.AddLine(mousePositionInData.X);
            }
            else if (sender.Equals(plotPitchNormal) || sender.Equals(plotYawNormal) || sender.Equals(plotRollNormal))
            {
                analyzeSpan.AddLine(mousePositionInData.X);
            }
            
            if (normalizeSpan.IsSet) {
                btnNormlize.IsEnabled = true;
            }
            if (analyzeSpan.IsSet) {
                analyseAngleNormalized();
            }
        }

        private void importAngleData(object sender, RoutedEventArgs args) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true) {
               string fileName = openFileDialog.FileName;
                using(FileStream fs = File.Open(fileName, FileMode.Open)){
                    if (!fileName.EndsWith(".GYT")) {
                        MessageBox.Show("姿态文件格式有误，请选择.gpx文件", "警告");
                        args.Handled = true;
                        return;
                    }
                    StreamReader sr = new StreamReader(fs);
                    string s;
                    try {
                        while ((s = sr.ReadLine()) != null)
                        {
                            string[] split = s.Split(',');
                            if (split.Length < 5)
                            {
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
                                FlightBean fb = new FlightBean();
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
                    catch {
                        MessageBox.Show("姿态文件格式有误", "警告");
                        args.Handled = true;
                        return;
                    }
                }
                indexDict = (from entry in indexDict orderby entry.Key ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
                // 绘图
                if (isReopenAngleFile) {
                    clearWhenReopenAngleFile();
                }
                plotAngle("pitch");
                plotAngle("yaw");
                plotAngle("roll");
                isReopenAngleFile = true;
                DispatcherTimer animationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
                animationTimer.Tick += animationTimer_Tick;
                animationTimer.Start();
            }
       }
        
        //点击轨迹按钮
        private void importTraceData(object sender, RoutedEventArgs args)
        {
            //打开选择文件窗口
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                // 轨迹数据读入
                string fileName = openFileDialog.FileName;
                //若选择的不是gpx文件，则报错
                if (fileName == null || !fileName.EndsWith(".gpx"))
                {
                    MessageBox.Show("轨迹文件格式错误，请选择.gpx文件","警告");
                    return;
                }
                //定义gpx_trans类对象，用来读GPX文件
                gpx_trans gpx = new gpx_trans(indexDict,flightBeanList);
                //开始读
                gpx.start(fileName);
                //等待读完再进行下一步操作
                while (gpx.thread1.ThreadState != System.Threading.ThreadState.Stopped) { }

                //读到的数据传给当前类的成员变量
                indexDict = gpx.gpxdata;
                flightBeanList = gpx.gpxlist;
                //将字典对象里的数据按时间排序
                indexDict = (from entry in indexDict orderby entry.Key ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
                traceChartPlotter.LegendVisible = false;

                if (analyzeSpan.IsSet)
                {
                    analyseAngleNormalized();
                }
                else
                {
                    plotTrace();
                }
            }
        }

        //绘轨迹图
        private void plotTrace()
        {
            //存横纵轴数据的列表
            List<double> latList = new List<double>();
            List<double> lngList = new List<double>();

            //获取轨迹数据
            foreach (string key in indexDict.Keys.Where(key => flightBeanList[indexDict[key]].lat != FlightBean.NoneCoordinate) )
            {
                    latList.Add(flightBeanList[indexDict[key]].lat);
                    lngList.Add(flightBeanList[indexDict[key]].lng);
            }

            if (latList.Count != lngList.Count || latList.Count == 0)
            {
                traceChartPlotter.Children.RemoveAll(typeof(LineGraph));
                return;
            }

            //将List数据转化为图的横纵轴数据并设置图的颜色和轮廓宽度
            EnumerableDataSource<double> latDataSource = new EnumerableDataSource<double>(latList);
            latDataSource.SetXMapping(y => y);
            EnumerableDataSource<double> logDataSource = new EnumerableDataSource<double>(lngList);
            logDataSource.SetYMapping(x => x);
            CompositeDataSource compositeDataSource = new CompositeDataSource(logDataSource, latDataSource);
            LineGraph lineG = new LineGraph()
            {
                Stroke = Brushes.Gray,
                StrokeThickness = 1,
                DataSource = compositeDataSource
            };

            //清空之前的图
            traceChartPlotter.Children.RemoveAll(lineG.GetType());
            traceChartPlotter.Viewport.FitToView();
            //加入要绘制的图
            traceChartPlotter.Children.Add(lineG);
            traceChartPlotter.LegendVisible = false;
        }

        private void plotAngle(string whichAngle) {
            // 例子，绘制俯仰角
            List<DateTime> dateTimeList = new List<DateTime>();
            List<double> angleList = new List<double>();
            if (whichAngle == "pitch") {
                foreach (string key in indexDict.Keys.Where(key => flightBeanList[indexDict[key]].pitch != FlightBean.NoneAngle) )
                {
                    dateTimeList.Add(TimeUtils.strToDateTime(key));
                    angleList.Add(flightBeanList[indexDict[key]].pitch);
                }

                EnumerableDataSource<DateTime> datesDataSource = new EnumerableDataSource<DateTime>(dateTimeList);
                datesDataSource.SetXMapping(x => dateAxisPitch.ConvertToDouble(x));
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
                foreach (string key in indexDict.Keys.Where(key => flightBeanList[indexDict[key]].pitch != FlightBean.NoneAngle))
                {
                    dateTimeList.Add(TimeUtils.strToDateTime(key));
                    angleList.Add(flightBeanList[indexDict[key]].yaw);
                }

                EnumerableDataSource<DateTime> datesDataSource = new EnumerableDataSource<DateTime>(dateTimeList);
                datesDataSource.SetXMapping(x => dateAxisYaw.ConvertToDouble(x));
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
                foreach (string key in indexDict.Keys.Where(key => flightBeanList[indexDict[key]].pitch != FlightBean.NoneAngle))
                {
                    dateTimeList.Add(TimeUtils.strToDateTime(key));
                    angleList.Add(flightBeanList[indexDict[key]].roll);
                }

                EnumerableDataSource<DateTime> datesDataSource = new EnumerableDataSource<DateTime>(dateTimeList);
                datesDataSource.SetXMapping(x => dateAxisRoll.ConvertToDouble(x));
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

        private void plotNormalizedAngle(string whichAngle)
        {
            // 例子，绘制俯仰角
            List<DateTime> dateTimeList = new List<DateTime>();
            List<double> angleList = new List<double>();
            if (whichAngle == "pitch")
            {
                foreach (string key in indexDict.Keys.Where(key => normalizedFlightBeanList[indexDict[key]].pitch != FlightBean.NoneAngle) )
                {
                    dateTimeList.Add(TimeUtils.strToDateTime(key));
                    angleList.Add(normalizedFlightBeanList[indexDict[key]].pitch);
                }

                EnumerableDataSource<DateTime> datesDataSource = new EnumerableDataSource<DateTime>(dateTimeList);
                datesDataSource.SetXMapping(x => dateAxisPitchNormal.ConvertToDouble(x));
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
                foreach (string key in indexDict.Keys.Where(key => normalizedFlightBeanList[indexDict[key]].pitch != FlightBean.NoneAngle) )
                {
                    dateTimeList.Add(TimeUtils.strToDateTime(key));
                    angleList.Add(normalizedFlightBeanList[indexDict[key]].yaw);
                }

                EnumerableDataSource<DateTime> datesDataSource = new EnumerableDataSource<DateTime>(dateTimeList);
                datesDataSource.SetXMapping(x => dateAxisYawNormal.ConvertToDouble(x));
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
                foreach (string key in indexDict.Keys.Where(key => normalizedFlightBeanList[indexDict[key]].pitch != FlightBean.NoneAngle) )
                {
                    dateTimeList.Add(TimeUtils.strToDateTime(key));
                    angleList.Add(normalizedFlightBeanList[indexDict[key]].roll);
                }

                EnumerableDataSource<DateTime> datesDataSource = new EnumerableDataSource<DateTime>(dateTimeList);
                datesDataSource.SetXMapping(x => dateAxisRollNormal.ConvertToDouble(x));
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

        private void onClick_NormalizeAngle(object sender = null, EventArgs e = null) {
            //获取时间区间
            DateTime dateTimeA = dateAxisPitch.ConvertFromDouble(normalizeSpan.valueOfLineA);
            DateTime dateTimeB = dateAxisPitch.ConvertFromDouble(normalizeSpan.valueOfLineB);

            List<FlightBean> list = null;
            if (indexDict.Keys.Count == 0) {
                return;
            }

            //超出时间范围，则校正为时间边界
            if (dateTimeA < TimeUtils.strToDateTime(indexDict.Keys.ElementAt(0)))
            {
                dateTimeA = TimeUtils.strToDateTime(indexDict.Keys.ElementAt(0));
            }
            else if (dateTimeA > TimeUtils.strToDateTime(indexDict.Keys.ElementAt(indexDict.Keys.Count - 1)))
            {
                dateTimeA = TimeUtils.strToDateTime(indexDict.Keys.ElementAt(indexDict.Keys.Count - 1));
            }
            if (dateTimeB < TimeUtils.strToDateTime(indexDict.Keys.ElementAt(0)))
            {
                dateTimeB = TimeUtils.strToDateTime(indexDict.Keys.ElementAt(0));
            }
            else if (dateTimeB > TimeUtils.strToDateTime(indexDict.Keys.ElementAt(indexDict.Keys.Count - 1)))
            {
                dateTimeB = TimeUtils.strToDateTime(indexDict.Keys.ElementAt(indexDict.Keys.Count - 1));
            }

            //获取截取的符合标准的链表
            if (dateTimeA < dateTimeB)
            {
                list = (from item in indexDict.Keys
                        where (dateTimeA < TimeUtils.strToDateTime(item)
                                && dateTimeB > TimeUtils.strToDateTime(item)
                                && flightBeanList[indexDict[item]].pitch != FlightBean.NoneAngle)
                        select flightBeanList[indexDict[item]]
                        ).ToList();
            }
            else
            {
                list = (from item in indexDict.Keys
                        where (dateTimeA > TimeUtils.strToDateTime(item)
                                && dateTimeB < TimeUtils.strToDateTime(item)
                                && flightBeanList[indexDict[item]].pitch != FlightBean.NoneAngle)
                        select flightBeanList[indexDict[item]]
                        ).ToList();
            }

            //如果没有符合标准的链表，则归一化的参数全部默认为零
            double meanOfPitch = 0, meanOfYaw = 0, meanOfRoll = 0;
            if (null != list && 0 != list.Count)
            {
                meanOfPitch = (from l in list select l.pitch).Sum() / list.Count;
                meanOfYaw = (from l in list select l.yaw).Sum() / list.Count;
                meanOfRoll = (from l in list select l.roll).Sum() / list.Count;
            }

            normalizedFlightBeanList = new List<FlightBean>(flightBeanList);
            foreach (FlightBean fb in normalizedFlightBeanList.Where(fb => fb.pitch != FlightBean.NoneAngle)) {
                fb.pitch -= meanOfPitch;
                fb.yaw -= meanOfYaw;
                fb.roll -= meanOfRoll;
            }
            plotNormalizedAngle("pitch");
            plotNormalizedAngle("yaw");
            plotNormalizedAngle("roll");
        }

        private void analyseAngleNormalized() {
            
            if (null == normalizedFlightBeanList)
                return;

            //获取时间区间
            DateTime dateTimeA = dateAxisPitchNormal.ConvertFromDouble(analyzeSpan.valueOfLineA);
            DateTime dateTimeB = dateAxisPitchNormal.ConvertFromDouble(analyzeSpan.valueOfLineB);

            List<FlightBean> list = null;

            //超出时间范围，则校正为时间边界
            if (dateTimeA < TimeUtils.strToDateTime(indexDict.Keys.ElementAt(0)))
            {
                dateTimeA = TimeUtils.strToDateTime(indexDict.Keys.ElementAt(0));
            }
            else if (dateTimeA > TimeUtils.strToDateTime(indexDict.Keys.ElementAt(indexDict.Keys.Count - 1)))
            {
                dateTimeA = TimeUtils.strToDateTime(indexDict.Keys.ElementAt(indexDict.Keys.Count - 1));
            }
            if (dateTimeB < TimeUtils.strToDateTime(indexDict.Keys.ElementAt(0)))
            {
                dateTimeB = TimeUtils.strToDateTime(indexDict.Keys.ElementAt(0));
            }
            else if (dateTimeB > TimeUtils.strToDateTime(indexDict.Keys.ElementAt(indexDict.Keys.Count - 1)))
            {
                dateTimeB = TimeUtils.strToDateTime(indexDict.Keys.ElementAt(indexDict.Keys.Count - 1));
            }

            //截取分析区间的数据列表
            if (dateTimeA < dateTimeB)
            {
                list = (from item in indexDict.Keys
                        where (dateTimeA < TimeUtils.strToDateTime(item)
                                && dateTimeB > TimeUtils.strToDateTime(item)
                                //normalizedFlightBeanList的个数可能与indexDict不同
                                && indexDict[item] < normalizedFlightBeanList.Count 
                                && normalizedFlightBeanList[indexDict[item]].pitch != FlightBean.NoneAngle)
                        select normalizedFlightBeanList[indexDict[item]]
                        ).ToList();
            }
            else
            {
                list = (from item in indexDict.Keys
                        where (dateTimeA > TimeUtils.strToDateTime(item)
                                && dateTimeB < TimeUtils.strToDateTime(item)
                                && indexDict[item] < normalizedFlightBeanList.Count  
                                && normalizedFlightBeanList[indexDict[item]].pitch != FlightBean.NoneAngle)
                        select normalizedFlightBeanList[indexDict[item]]
                        ).ToList();
            }

            //无符合标准的点，则不分析
            if(null == list || 0 == list.Count)
            {
                plotTrace();
                return;
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
            List<double> latListHLight = (from item in list where item.lat != FlightBean.NoneCoordinate select item.lat).ToList();
            List<double> lngListHLight = (from item in list where item.lng != FlightBean.NoneCoordinate select item.lng).ToList();
            //无可以高亮的轨迹，则不高亮
            if (latListHLight.Count != lngListHLight.Count || latListHLight.Count == 0)
            {
                plotTrace();
                return;
            }

            EnumerableDataSource<double> latHLightDataSource = new EnumerableDataSource<double>(latListHLight);
            latHLightDataSource.SetXMapping(y => y);
            EnumerableDataSource<double> logHLightDataSource = new EnumerableDataSource<double>(lngListHLight);
            logHLightDataSource.SetYMapping(x => x);
            CompositeDataSource compositeHLightDataSource = new CompositeDataSource(logHLightDataSource, latHLightDataSource);
            LineGraph lineGHLight = new LineGraph
            {
                Name = "TraceHLight",
                Stroke = Brushes.Blue,
                StrokeThickness = 3,
                DataSource = compositeHLightDataSource
            };
            plotTrace();
            traceChartPlotter.Children.Add(lineGHLight);
            traceChartPlotter.LegendVisible = false;
        }

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            Point pos = new Point(dateAxisPitch.ConvertToDouble(TimeUtils.strToDateTime(TimeUtils.toformatTime("160529163534"))), 0);
            Point zoomTo = pos.DataToScreen(plotPitch.Viewport.Transform);
            plotPitch.Viewport.Visible.Zoom(zoomTo, 3.0);
        }

        private void clearWhenReopenAngleFile()
        {
            plotPitch.Children.RemoveAll(typeof(LineGraph));
            plotYaw.Children.RemoveAll(typeof(LineGraph));
            plotRoll.Children.RemoveAll(typeof(LineGraph));
            plotPitchNormal.Children.RemoveAll(typeof(LineGraph));
            plotYawNormal.Children.RemoveAll(typeof(LineGraph));
            plotRollNormal.Children.RemoveAll(typeof(LineGraph));
            normalizeSpan.Reset();
            analyzeSpan.Reset();
            clearanalysisTextBox();
            btnNormlize.IsEnabled = false;
            if (((LineGraph)traceChartPlotter.FindName("traceHLight")) != null) {
                traceChartPlotter.Children.Remove((LineGraph)FindName("traceHLight"));
                traceChartPlotter.UnregisterName("traceHLight");
            }
        }

        private void clearanalysisTextBox() {
            pitchMaxTextBox.Text = "";
            yawMaxTextBox.Text = "";
            rollMaxTextBox.Text = "";
            pitchMinTextBox.Text = "";
            yawMinTextBox.Text = "";
            rollMinTextBox.Text = "";
        }
    }
}
