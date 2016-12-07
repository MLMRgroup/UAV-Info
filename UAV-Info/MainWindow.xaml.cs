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
using Microsoft.Research.DynamicDataDisplay.PointMarkers;
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

        // isReopenAngleFile = true表示重新打开姿态文件
        private bool isReopenAngleFile;

        // isReopenTraceFile = true表示重新打开轨迹文件
        private bool isReopenTraceFile;

        // 选择的分析区间中，姿态角的最值
        FlightBean maxOfPitch = null;
        FlightBean maxOfYaw = null;
        FlightBean maxOfRoll = null;
        FlightBean minOfPitch = null;
        FlightBean minOfYaw = null;
        FlightBean minOfRoll = null;

        private bool firstchange;

        // 存储用来恢复zoom的数据
        private Rect initialViewPortVisible;

        public MainWindow()
        {
            InitializeComponent();
            //设置背景颜色
            SolidColorBrush NewColor = new SolidColorBrush();
            NewColor.Color = Color.FromArgb(255, 110, 204, 210);
            this.Background = NewColor;

            flightBeanList = new List<FlightBean>();
            indexDict = new Dictionary<string, int>();
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
            isReopenAngleFile = false;
            isReopenTraceFile = false;
            firstchange = true;

            // 调整使数据图有外边框
            traceChartPlotter.MainGrid.Margin = new Thickness(0, 10, 10, 0);
            plotPitch.MainGrid.Margin = new Thickness(0, 10, 10, 0);
            plotYaw.MainGrid.Margin = new Thickness(0, 10, 10, 0);
            plotRoll.MainGrid.Margin = new Thickness(0, 10, 10, 0);
            plotPitchNormal.MainGrid.Margin = new Thickness(0, 10, 10, 0);
            plotYawNormal.MainGrid.Margin = new Thickness(0, 10, 10, 0);
            plotRollNormal.MainGrid.Margin = new Thickness(0, 10, 10, 0);

            // 去除图中横向纵向的格子
            traceChartPlotter.AxisGrid.DrawHorizontalTicks = false;
            traceChartPlotter.AxisGrid.DrawVerticalTicks = false;
            plotPitch.AxisGrid.DrawHorizontalTicks = false;
            plotPitch.AxisGrid.DrawVerticalTicks = false;
            plotYaw.AxisGrid.DrawHorizontalTicks = false;
            plotYaw.AxisGrid.DrawVerticalTicks = false;
            plotRoll.AxisGrid.DrawHorizontalTicks = false;
            plotRoll.AxisGrid.DrawVerticalTicks = false;
            plotPitchNormal.AxisGrid.DrawHorizontalTicks = false;
            plotPitchNormal.AxisGrid.DrawVerticalTicks = false;
            plotYawNormal.AxisGrid.DrawHorizontalTicks = false;
            plotYawNormal.AxisGrid.DrawVerticalTicks = false;
            plotRollNormal.AxisGrid.DrawHorizontalTicks = false;
            plotRollNormal.AxisGrid.DrawVerticalTicks = false;

            // 初始化轨迹图的zoom
            initialViewPortVisible = new Rect(traceChartPlotter.Viewport.Visible.X, traceChartPlotter.Viewport.Visible.Y, traceChartPlotter.Viewport.Visible.Width, traceChartPlotter.Viewport.Visible.Height);
        }

        //normalizeSpan：规范化选取区间  analyzeSpan：分析选取区间
        Span normalizeSpan = new Span();
        Span timeSpan = new Span();
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

            // 删去双击放大事件
            plotPitch.Children.Remove(plotPitch.KeyboardNavigation);
            plotYaw.Children.Remove(plotYaw.KeyboardNavigation);
            plotRoll.Children.Remove(plotRoll.KeyboardNavigation);
            plotPitchNormal.Children.Remove(plotPitchNormal.KeyboardNavigation);
            plotYawNormal.Children.Remove(plotYawNormal.KeyboardNavigation);
            plotRollNormal.Children.Remove(plotRollNormal.KeyboardNavigation);
            traceChartPlotter.Children.Remove(traceChartPlotter.KeyboardNavigation);

            // 添加双击描线事件，双击之后出现一条红线
            plotPitch.MouseDoubleClick += onDoubleCkick_AngleChart;
            plotYaw.MouseDoubleClick += onDoubleCkick_AngleChart;
            plotRoll.MouseDoubleClick += onDoubleCkick_AngleChart;

            plotPitchNormal.MouseDoubleClick += onDoubleCkick_AngleChart;
            plotYawNormal.MouseDoubleClick += onDoubleCkick_AngleChart;
            plotRollNormal.MouseDoubleClick += onDoubleCkick_AngleChart;

            // 添加坐标中的日期显示格式
            cordPitch.XTextMapping = x => dateAxisPitch.ConvertFromDouble(x).ToString("HH:mm:ss");
            cordYaw.XTextMapping = x => dateAxisYaw.ConvertFromDouble(x).ToString("HH:mm:ss");
            cordRoll.XTextMapping = x => dateAxisRoll.ConvertFromDouble(x).ToString("HH:mm:ss");
            cordPitchNormal.XTextMapping = x => dateAxisPitchNormal.ConvertFromDouble(x).ToString("HH:mm:ss");
            cordYawNormal.XTextMapping = x => dateAxisYawNormal.ConvertFromDouble(x).ToString("HH:mm:ss");
            cordRollNormal.XTextMapping = x => dateAxisRollNormal.ConvertFromDouble(x).ToString("HH:mm:ss");
            traceChartPlotter.PreviewKeyDown += Zoom;
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
            if (sender.Equals(btnNormlizeSpanReset))
            {
                normalizeSpan.Reset();
                btnNormlize.IsEnabled = false;
            }
            //重置分析区间
            else if (sender.Equals(btnAnalyzeSpanReset))
            {
                analyzeSpan.Reset();
                clearanalysisTextBox();
                clearHLight();
            }
        }

        //窗口同步，暂时用不到
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

        //窗口双击事件
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
                // 在原始图中添加基准线
                if (normalizeSpan.IsSet)
                {
                    return;
                }
                normalizeSpan.AddLine(mousePositionInData.X);
                if (normalizeSpan.IsSet)
                {
                    btnNormlize.IsEnabled = true;
                }
            }
            else if (sender.Equals(plotPitchNormal) || sender.Equals(plotYawNormal) || sender.Equals(plotRollNormal))
            {
                // 在规范化之后的图中添加基准线
                if (analyzeSpan.IsSet)
                {
                    return;
                }
                analyzeSpan.AddLine(mousePositionInData.X);
                if (analyzeSpan.IsSet)
                {
                    analyseAngleNormalized();
                }
            }
        }

        /// <summary>
        /// 导入无人机姿态数据，需保证文件格式正确，并以.GYT为文件后缀
        /// 数据存入flightBeanList中，并用indexDict为索引存储
        /// 导入数据成功之后，进行无人机姿态角图形的绘制，包括三个维度的姿态角：俯仰，偏航，滚转
        /// </summary>
        /// <param name="sender">事件对象</param>
        /// <param name="args">事件参数</param>
        private void importAngleData(object sender, RoutedEventArgs args)
        {
            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                if (isReopenAngleFile)
                {
                    // 再次点击`打开姿态数据文件`按钮
                    clearWhenReopenAngleFile();
                    ClearWorkSpace();
                    isReopenTraceFile = false;
                }
                else {
                    isReopenAngleFile = true;
                }
                // 获取文件名
                string fileName = openFileDialog.FileName;
                using (FileStream fs = File.Open(fileName, FileMode.Open))
                {
                    // 必须读取以.GYT为后缀的文件
                    if (!fileName.EndsWith(".GYT"))
                    {
                        MessageBox.Show("姿态文件格式有误，请选择.gpx文件", "警告");
                        args.Handled = true;
                        return;
                    }
                    StreamReader sr = new StreamReader(fs);
                    string s;
                    try
                    {
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
                                // 如果已经读取了轨迹数据，在indexDict中已经有time这个key值，则修改FlightBean的姿态数据
                                FlightBean fb = flightBeanList[indexDict[time]];
                                fb.pitch = Convert.ToDouble(split[2]);
                                fb.yaw = Convert.ToDouble(split[4]);
                                fb.roll = Convert.ToDouble(split[3]);
                            }
                            else
                            {
                                // 如果未读取了轨迹数据，则新增FlightBean的姿态数据，轨迹数据为默认值
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
                    catch
                    {
                        MessageBox.Show("姿态文件格式有误", "警告");
                        args.Handled = true;
                        return;
                    }
                }
                indexDict = (from entry in indexDict orderby entry.Key ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);
                // 绘制无人机姿态角数据图，pitch: 俯仰，yaw：偏航，roll:滚转
                plotAngle("pitch");
                plotAngle("yaw");
                plotAngle("roll");
                //isReopenAngleFile = true;
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
                if (isReopenTraceFile)
                {
                    // 重新打开的时候需要清除所有工作空间的数据
                    //clearWhenReopenAngleFile();
                    ClearWorkSpace();
                    isReopenAngleFile = false;
                }
                else
                {
                    isReopenTraceFile = true;
                }
                //isReopenTraceFile = true;
                // 轨迹数据读入
                string fileName = openFileDialog.FileName;
                //若选择的不是gpx文件，则报错
                if (fileName == null || !fileName.EndsWith(".gpx"))
                {
                    MessageBox.Show("轨迹文件格式错误，请选择.gpx文件", "警告");
                    return;
                }
                //定义gpx_trans类对象，用来读GPX文件
                gpx_trans gpx = new gpx_trans(indexDict, flightBeanList);
                //开始读
                gpx.start(fileName);
                //等待读完再进行下一步操作
                while (gpx.thread1.ThreadState != System.Threading.ThreadState.Stopped) { }

                //读到的数据传给当前类的成员变量
                indexDict = gpx.gpxdata;
                flightBeanList = gpx.gpxlist;
                //将字典对象里的数据按时间排序
                indexDict = (from entry in indexDict orderby entry.Key ascending select entry).ToDictionary(pair => pair.Key, pair => pair.Value);

                // 调用绘制轨迹图函数绘制二维轨迹图
                plotTrace();

                if (analyzeSpan.IsSet)
                {
                    // 如果已经通过双击得到了分析区间，则在绘制完Trace之后，调用analyseAngleNormalized函数
                    analyseAngleNormalized();
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
            foreach (string key in indexDict.Keys.Where(key => flightBeanList[indexDict[key]].lat != FlightBean.NoneCoordinate))
            {
                latList.Add(flightBeanList[indexDict[key]].lat);
                lngList.Add(flightBeanList[indexDict[key]].lng);
            }

            if (latList.Count != lngList.Count || latList.Count == 0)
            {
                traceChartPlotter.Children.RemoveAll(typeof(LineGraph));
                return;
            }

            //清空之前的图
            clearHLight();

            if (((LineGraph)traceChartPlotter.FindName("traceOrdinary")) != null)
            {
                traceChartPlotter.Children.Remove((LineGraph)FindName("traceOrdinary"));
                traceChartPlotter.UnregisterName("traceOrdinary");
            }

            //将List数据转化为图的横纵轴数据并设置图的颜色和轮廓宽度
            EnumerableDataSource<double> latDataSource = new EnumerableDataSource<double>(latList);
            latDataSource.SetXMapping(y => y);
            EnumerableDataSource<double> logDataSource = new EnumerableDataSource<double>(lngList);
            logDataSource.SetYMapping(x => x);
            CompositeDataSource compositeDataSource = new CompositeDataSource(logDataSource, latDataSource);
            
            LineGraph lineG = new LineGraph()
            {
                Name = "traceOrdinary",
                Stroke = Brushes.Gray,
                StrokeThickness = 1,
                DataSource = compositeDataSource
            };
            if (((LineGraph)traceChartPlotter.FindName("traceOrdinary")) == null)
            {
                lineG.ZIndex = -1;
                traceChartPlotter.RegisterName("traceOrdinary", lineG);
            }
            traceChartPlotter.Viewport.FitToView();
            //加入要绘制的图
            traceChartPlotter.Children.Add(lineG);
            traceChartPlotter.LegendVisible = false;
        }

        /// <summary>
        /// 绘制三个姿态角的数据图，内部函数
        /// </summary>
        /// <param name="whichAngle">当前绘制的姿态角，可选姿态角：pitch,yaw,roll</param>
        /// <example>plotAngle(pitch);// 绘制俯仰角姿态的数据图</example>
        private void plotAngle(string whichAngle)
        {
            // 每次绘制姿态数据图的时候，清除掉区间红线
            normalizeSpan.Reset();
            analyzeSpan.Reset();

            List<DateTime> dateTimeList = new List<DateTime>();
            List<double> angleList = new List<double>();
            if (whichAngle == "pitch")
            {
                // 绘制俯仰角数据图
                foreach (string key in indexDict.Keys.Where(key => flightBeanList[indexDict[key]].pitch != FlightBean.NoneAngle))
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
                lineG.ZIndex = -1;
                plotPitch.Children.RemoveAll(lineG.GetType());
                plotPitch.Viewport.FitToView();
                plotPitch.Children.Add(lineG);
            }
            else if (whichAngle == "yaw")
            {
                // 绘制偏航角数据图
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
                lineG.ZIndex = -1;
                lineG.Description = new PenDescription("偏航");
                lineG.DataSource = compositeDataSource;
                plotYaw.Children.RemoveAll(lineG.GetType());
                plotYaw.Viewport.FitToView();
                plotYaw.Children.Add(lineG);
            }
            else
            {
                // 绘制滚转角数据图
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
                // 将折线图放在最后边
                lineG.ZIndex = -1;
                lineG.Description = new PenDescription("滚转");
                lineG.DataSource = compositeDataSource;
                plotRoll.Children.RemoveAll(lineG.GetType());
                plotRoll.Viewport.FitToView();
                plotRoll.Children.Add(lineG);
            }
        }

        /// <summary>
        /// 绘制规范化之后的姿态数据图
        /// 在成功导入数据并绘制原始姿态数据图之后，选择时间域，以此做规范化，并绘制规范化后的姿态数据图
        /// </summary>
        /// <param name="whichAngle">当前绘制的姿态角，可选姿态角：pitch,yaw,roll</param>
        /// <example>plotNormalizedAngle(pitch);// 绘制俯仰角姿态的规范化数据图</example>
        private void plotNormalizedAngle(string whichAngle)
        {
            // 例子，绘制俯仰角
            List<DateTime> dateTimeList = new List<DateTime>();
            List<double> angleList = new List<double>();
            if (whichAngle == "pitch")
            {
                // 绘制俯仰角数据图
                foreach (string key in indexDict.Keys.Where(key => normalizedFlightBeanList[indexDict[key]].pitch != FlightBean.NoneAngle))
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
                lineG.ZIndex = -1;
                lineG.Description = new PenDescription("俯仰");
                lineG.DataSource = compositeDataSource;
                plotPitchNormal.Children.RemoveAll(lineG.GetType());
                plotPitchNormal.Viewport.FitToView();
                plotPitchNormal.Children.Add(lineG);
            }
            else if (whichAngle == "yaw")
            {
                // 绘制偏航角数据图
                foreach (string key in indexDict.Keys.Where(key => normalizedFlightBeanList[indexDict[key]].pitch != FlightBean.NoneAngle))
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
                lineG.ZIndex = -1;
                lineG.Description = new PenDescription("偏航");
                lineG.DataSource = compositeDataSource;
                plotYawNormal.Children.RemoveAll(lineG.GetType());
                plotYawNormal.Viewport.FitToView();
                plotYawNormal.Children.Add(lineG);
            }
            else
            {
                // 绘制滚转角数据图
                foreach (string key in indexDict.Keys.Where(key => normalizedFlightBeanList[indexDict[key]].pitch != FlightBean.NoneAngle))
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
                lineG.ZIndex = -1;
                lineG.Description = new PenDescription("滚转");
                lineG.DataSource = compositeDataSource;
                plotRollNormal.Children.RemoveAll(lineG.GetType());
                plotRollNormal.Viewport.FitToView();
                plotRollNormal.Children.Add(lineG);
            }

        }

        /// <summary>
        /// 在点击了规范化之后，根据在原始姿态图中所选择的区域，得到用作校准的时间区域，分割得到区域中的
        /// 所需数据，并计算零点值，调用plotNormalizedAngle函数绘制规范化之后的姿态数据图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onClick_NormalizeAngle(object sender = null, EventArgs e = null)
        {
            //获取时间区间
            DateTime dateTimeA = dateAxisPitch.ConvertFromDouble(normalizeSpan.valueOfLineA);
            DateTime dateTimeB = dateAxisPitch.ConvertFromDouble(normalizeSpan.valueOfLineB);

            List<FlightBean> list = null;
            if (indexDict.Keys.Count == 0)
            {
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

            normalizedFlightBeanList = new List<FlightBean>();
            foreach (FlightBean fb in flightBeanList)
            {
                normalizedFlightBeanList.Add(new FlightBean(fb.time, fb.lat, fb.lng, fb.pitch, fb.yaw, fb.roll));
            }

            // 得到规范化的姿态数据，ie：减去均值
            foreach (FlightBean fb in normalizedFlightBeanList.Where(fb => fb.pitch != FlightBean.NoneAngle))
            {
                fb.pitch -= meanOfPitch;
                fb.yaw -= meanOfYaw;
                fb.roll -= meanOfRoll;
            }

            //重新绘制规范化图形，则清空之前所有依据规范化图形得到的值
            analyzeSpan.Reset();
            clearanalysisTextBox();
            clearHLight();

            // 得到数据之后，绘制规范化之后的姿态图形
            plotNormalizedAngle("pitch");
            plotNormalizedAngle("yaw");
            plotNormalizedAngle("roll");
        }

        /// <summary>
        /// 在规范化的姿态数据图中选择时间区域后，将在轨迹图中对应时间区域的部分进行高亮显示，
        /// 并计算零点值，得到该分析区间得到姿态角的最大值最小值并显示
        /// </summary>
        private void analyseAngleNormalized()
        {

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
            if (null == list || 0 == list.Count)
            {
                clearHLight();
                clearanalysisTextBox();
                return;
            }

            maxOfPitch = list[0];
            maxOfYaw = list[0];
            maxOfRoll = list[0];
            minOfPitch = list[0];
            minOfYaw = list[0];
            minOfRoll = list[0];

            // 得到三个姿态角的最值
            foreach (FlightBean bean in list)
            {
                maxOfPitch = bean.pitch > maxOfPitch.pitch ? bean : maxOfPitch;
                maxOfYaw = bean.yaw > maxOfYaw.yaw ? bean : maxOfYaw;
                maxOfRoll = bean.roll > maxOfRoll.roll ? bean : maxOfRoll;
                minOfPitch = bean.pitch > minOfPitch.pitch ? minOfPitch : bean;
                minOfYaw = bean.yaw > minOfYaw.yaw ? minOfYaw : bean;
                minOfRoll = bean.roll > minOfRoll.roll ? minOfRoll : bean;
            }

            // double maxOfPitch = (from l in list select l.pi).Max();
            // double minOfPitch = (from l in list select l.pitch).Min();
            // double maxOfYaw = (from l in list select l.yaw).Max();
            // double minOfYaw = (from l in list select l.yaw).Min();
            // double maxOfRoll = (from l in list select l.roll).Max();
            // double minOfRoll = (from l in list select l.roll).Min();

            // 把三个姿态角的数据填入各自的文本框显示
            pitchMaxTextBox.Text = maxOfPitch.pitch.ToString("f2");
            yawMaxTextBox.Text = maxOfYaw.yaw.ToString("f2");
            rollMaxTextBox.Text = maxOfRoll.roll.ToString("f2");
            pitchMinTextBox.Text = minOfPitch.pitch.ToString("f2");
            yawMinTextBox.Text = minOfYaw.yaw.ToString("f2");
            rollMinTextBox.Text = minOfRoll.roll.ToString("f2");

            // 在规范化的三个姿态角数据图中，显示出分析区间中的最值点，每个最值处用带颜色的圆形表示
            addMarkerOnPlotter(plotPitchNormal, maxOfPitch.time, maxOfPitch.pitch, "maxOfPitch", Brushes.Green);
            addMarkerOnPlotter(plotPitchNormal, minOfPitch.time, minOfPitch.pitch, "minOfPitch", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF48FF04")));
            addMarkerOnPlotter(plotYawNormal, maxOfYaw.time, maxOfYaw.yaw, "maxOfYaw", Brushes.Black);
            addMarkerOnPlotter(plotYawNormal, minOfYaw.time, minOfYaw.yaw, "minOfYaw", Brushes.Purple);
            addMarkerOnPlotter(plotRollNormal, maxOfRoll.time, maxOfRoll.roll, "maxOfRoll", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF509AA")));
            addMarkerOnPlotter(plotRollNormal, minOfRoll.time, minOfRoll.roll, "minOfRoll", Brushes.Firebrick);

            // 在轨迹图中，显示出分析区间中的最值点对应的经纬度位置，每个位置用带颜色的圆形表示
            addMarkerOnTrace(maxOfPitch, "maxOfPitch_trace", Brushes.Green);
            addMarkerOnTrace(minOfPitch, "minOfPitch_trace", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF48FF04")));
            addMarkerOnTrace(maxOfYaw, "maxOfYaw_trace", Brushes.Black);
            addMarkerOnTrace(minOfYaw, "minOfYaw_trace", Brushes.Purple);
            addMarkerOnTrace(maxOfRoll, "maxOfRoll_trace", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF509AA")));
            addMarkerOnTrace(minOfRoll, "minOfRoll_trace", Brushes.Firebrick);

            //HighLight The Trace
            List<double> latListHLight = (from item in list where item.lat != FlightBean.NoneCoordinate select item.lat).ToList();
            List<double> lngListHLight = (from item in list where item.lng != FlightBean.NoneCoordinate select item.lng).ToList();
            //无可以高亮的轨迹，则不高亮
            if (latListHLight.Count != lngListHLight.Count || latListHLight.Count == 0)
            {
                clearGraph(traceChartPlotter, "traceHLight");
                return;
            }

            EnumerableDataSource<double> latHLightDataSource = new EnumerableDataSource<double>(latListHLight);
            latHLightDataSource.SetXMapping(y => y);
            EnumerableDataSource<double> logHLightDataSource = new EnumerableDataSource<double>(lngListHLight);
            logHLightDataSource.SetYMapping(x => x);
            CompositeDataSource compositeHLightDataSource = new CompositeDataSource(logHLightDataSource, latHLightDataSource);
            LineGraph lineGHLight = new LineGraph
            {
                Name = "traceHLight",
                Stroke = Brushes.Blue,
                StrokeThickness = 3,
                DataSource = compositeHLightDataSource
            };
            lineGHLight.DataSource = compositeHLightDataSource;

            // 高亮显示分析区间的轨迹
            if (((LineGraph)traceChartPlotter.FindName("traceHLight")) == null)
            {
                traceChartPlotter.RegisterName("traceHLight", lineGHLight);
            }
            lineGHLight.ZIndex = -1;
            traceChartPlotter.Children.Add(lineGHLight);
            traceChartPlotter.LegendVisible = false;
        }

        /// <summary>
        /// 在三个规范化的姿态数据图中，根据函数传入的横纵坐标显示一个小的带颜色圆形，表示每个最值
        /// </summary>
        /// <param name="plotter"></param>
        /// <param name="valueX">坐标系的x轴坐标</param>
        /// <param name="valueY">坐标系的y轴坐标</param>
        /// <param name="description">每个圆形的名字</param>
        /// <param name="brush">颜色</param>
        private void addMarkerOnPlotter(ChartPlotter plotter, string valueX, double valueY, string description, Brush brush)
        {
            List<DateTime> dateTimeList = new List<DateTime>();
            List<double> angleList = new List<double>();
            dateTimeList.Add(TimeUtils.strToDateTime(valueX));
            angleList.Add(valueY);

            EnumerableDataSource<DateTime> datesDataSource = new EnumerableDataSource<DateTime>(dateTimeList);
            datesDataSource.SetXMapping(x => dateAxisPitchNormal.ConvertToDouble(x));
            EnumerableDataSource<double> angleDataSource = new EnumerableDataSource<double>(angleList);
            angleDataSource.SetYMapping(y => y);
            CompositeDataSource compositeDataSource = new CompositeDataSource(datesDataSource, angleDataSource);

            MarkerPointsGraph markerGraph = new MarkerPointsGraph
            {
                Name = null,
                Marker = new CirclePointMarker { Size = 10, Fill = brush },
                DataSource = compositeDataSource
            };
            markerGraph.ZIndex = -1;
            if (((MarkerPointsGraph)plotter.FindName(description)) == null)
            {
                plotter.RegisterName(description, markerGraph);
            }
            plotter.Children.Add(markerGraph);
        }

        /// <summary>
        /// 在轨迹图中显示6个圆形，每个圆形，表示分析区间内的姿态最值对应时刻无人机的轨迹点
        /// </summary>
        /// <param name="bean">无人机飞行数据</param>
        /// <param name="description">每个圆形的名字</param>
        /// <param name="brush">圆形de颜色</param>
        private void addMarkerOnTrace(FlightBean bean, string description, Brush brush)
        {
            List<double> latList = new List<double>();
            List<double> lngList = new List<double>();
            if (FlightBean.NoneCoordinate == bean.lat)
            {
                return;
            }
            latList.Add(bean.lat);
            lngList.Add(bean.lng);

            EnumerableDataSource<double> latDataSource = new EnumerableDataSource<double>(latList);
            latDataSource.SetXMapping(y => y);
            EnumerableDataSource<double> lngDataSource = new EnumerableDataSource<double>(lngList);
            lngDataSource.SetYMapping(x => x);
            CompositeDataSource compositeDataSource = new CompositeDataSource(latDataSource, lngDataSource);

            MarkerPointsGraph markerGraph = new MarkerPointsGraph
            {
                Name = null,
                Marker = new CirclePointMarker { Size = 15, Fill = brush },
                DataSource = compositeDataSource
            };
            markerGraph.ZIndex = -1;
            if (((MarkerPointsGraph)traceChartPlotter.FindName(description)) == null)
            {
                traceChartPlotter.RegisterName(description, markerGraph);
            }
            traceChartPlotter.Children.Add(markerGraph);
        }
        /// <summary>
        /// 将姿态数据图zoom到合适的范围
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void animationTimer_Tick(object sender, EventArgs e)
        {
            Point pos = new Point(dateAxisPitch.ConvertToDouble(TimeUtils.strToDateTime(TimeUtils.toformatTime("160529163534"))), 0);
            Point zoomTo = pos.DataToScreen(plotPitch.Viewport.Transform);
            plotPitch.Viewport.Visible.Zoom(zoomTo, 3.0);
        }

        /// <summary>
        /// 当重新打开姿态文件之后，需要清空已经绘制的姿态数据曲线，以及选择好的时间区域（以红线标注），
        /// 还有对轨迹中的高亮区域去除高亮，清空姿态数据展示的6个文本框
        /// </summary>
        private void clearWhenReopenAngleFile()
        {
            plotPitch.Children.RemoveAll(typeof(LineGraph));
            plotYaw.Children.RemoveAll(typeof(LineGraph));
            plotRoll.Children.RemoveAll(typeof(LineGraph));
            plotPitchNormal.Children.RemoveAll(typeof(LineGraph));
            plotYawNormal.Children.RemoveAll(typeof(LineGraph));
            plotRollNormal.Children.RemoveAll(typeof(LineGraph));
            clearanalysisTextBox();
            clearHLight();
            btnNormlize.IsEnabled = false;
        }

        /// <summary>
        /// 重置之后，或重新导入姿态数据文件，需清空三个姿态角，共6个文本框的文本
        /// </summary>
        private void clearanalysisTextBox()
        {
            pitchMaxTextBox.Text = "";
            yawMaxTextBox.Text = "";
            rollMaxTextBox.Text = "";
            pitchMinTextBox.Text = "";
            yawMinTextBox.Text = "";
            rollMinTextBox.Text = "";
        }

        /// <summary>
        /// 清除高亮轨迹
        /// </summary>
        private void clearHLight()
        {
            clearGraph(traceChartPlotter, "traceHLight");
            clearGraph(plotPitchNormal, "maxOfPitch");
            clearGraph(plotYawNormal, "maxOfYaw");
            clearGraph(plotRollNormal, "maxOfRoll");
            clearGraph(plotPitchNormal, "minOfPitch");
            clearGraph(plotYawNormal, "minOfYaw");
            clearGraph(plotRollNormal, "minOfRoll");

            clearGraph(traceChartPlotter, "maxOfPitch_trace");
            clearGraph(traceChartPlotter, "maxOfYaw_trace");
            clearGraph(traceChartPlotter, "maxOfRoll_trace");
            clearGraph(traceChartPlotter, "minOfPitch_trace");
            clearGraph(traceChartPlotter, "minOfYaw_trace");
            clearGraph(traceChartPlotter, "minOfRoll_trace");

            traceChartPlotter.LegendVisible = false;
        }

        /// <summary>
        /// 清除图中高亮轨迹或最大最小标注点
        /// </summary>
        /// <param name="plotter"></param>
        /// <param name="description"></param>
        private void clearGraph(ChartPlotter plotter, string description)
        {
            if (plotter.FindName(description) != null)
            {
                if (description == "traceHLight")
                    plotter.Children.Remove((LineGraph)FindName(description));
                else
                    plotter.Children.Remove((MarkerPointsGraph)FindName(description));
                plotter.UnregisterName(description);
            }
        }

        /// <summary>
        /// 滑动缩放条的处理逻辑，向上则zoom in, 向下则zoom out
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double step = (e.NewValue - e.OldValue) / 50.0;
            if(step > 0.3)
            {
                step = 0.3;
            }
            if (e.NewValue == sliderTraceZoom.Minimum) {
                traceChartPlotter.Viewport.Visible = initialViewPortVisible;
                return;
            }
            double unitX = traceChartPlotter.Viewport.Visible.Width * step;
            double unitY = traceChartPlotter.Viewport.Visible.Height * step;
            traceChartPlotter.Viewport.Visible = new Rect(traceChartPlotter.Viewport.Visible.X + unitX, traceChartPlotter.Viewport.Visible.Y + unitY, traceChartPlotter.Viewport.Visible.Width - 2 * unitX, traceChartPlotter.Viewport.Visible.Height - 2 * unitY);
        }

        /// <summary>
        /// 键盘的+,-符号键也进行缩放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Zoom(object sender, KeyEventArgs e)
        {
            double unitX = traceChartPlotter.Viewport.Visible.Width * 0.02;
            double unitY = traceChartPlotter.Viewport.Visible.Height * 0.02;
            if (System.Windows.Input.Key.Add == e.Key)
            {
                traceChartPlotter.Viewport.Visible = new Rect(traceChartPlotter.Viewport.Visible.X + unitX, traceChartPlotter.Viewport.Visible.Y + unitY, traceChartPlotter.Viewport.Visible.Width - 2 * unitX, traceChartPlotter.Viewport.Visible.Height - 2 * unitY);
            }
            else if (System.Windows.Input.Key.Subtract == e.Key)
            {
                traceChartPlotter.Viewport.Visible = new Rect(traceChartPlotter.Viewport.Visible.X - unitX, traceChartPlotter.Viewport.Visible.Y - unitY, traceChartPlotter.Viewport.Visible.Width + 2 * unitX, traceChartPlotter.Viewport.Visible.Height + 2 * unitY);
            }
        }

        /// <summary>
        /// 双击轨迹图之后，显示出放大缩小的sliderbar，再次双击时，sliderbar消失
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scaleMap(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sliderTraceZoom.Visibility == Visibility.Visible)
                {
                    sliderTraceZoom.Visibility = Visibility.Hidden;
                }
                else
                {
                    sliderTraceZoom.Visibility = Visibility.Visible;
                    initialViewPortVisible = new Rect(traceChartPlotter.Viewport.Visible.X, traceChartPlotter.Viewport.Visible.Y, traceChartPlotter.Viewport.Visible.Width, traceChartPlotter.Viewport.Visible.Height);
                }
            }
            e.Handled = true;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (firstchange)
            {
                firstchange = false;
                return;
            }
            //窗口缩放比例
            double Hscale=0,Wscale=0;
            Hscale = e.NewSize.Height / e.PreviousSize.Height;
            Wscale = e.NewSize.Width / e.PreviousSize.Width;

            //设置图的大小和位置
            traceChartPlotter.Height = Hscale * traceChartPlotter.Height;
            traceChartPlotter.Width = Wscale * traceChartPlotter.Width;
            traceChartPlotter.Margin = setMargin(traceChartPlotter.Margin,Hscale,Wscale);

            plotRoll.Height = Hscale * plotRoll.Height;
            plotRoll.Width = Wscale * plotRoll.Width;
            plotRoll.Margin = setMargin(plotRoll.Margin, Hscale, Wscale);

            plotYaw.Height = Hscale * plotYaw.Height;
            plotYaw.Width = Wscale * plotYaw.Width;
            plotYaw.Margin = setMargin(plotYaw.Margin, Hscale, Wscale);

            plotPitch.Height = Hscale * plotPitch.Height;
            plotPitch.Width = Wscale * plotPitch.Width;
            plotPitch.Margin = setMargin(plotPitch.Margin, Hscale, Wscale);

            plotRollNormal.Height = Hscale * plotRollNormal.Height;
            plotRollNormal.Width = Wscale * plotRollNormal.Width;
            plotRollNormal.Margin = setMargin(plotRollNormal.Margin, Hscale, Wscale);

            plotYawNormal.Height = Hscale * plotYawNormal.Height;
            plotYawNormal.Width = Wscale * plotYawNormal.Width;
            plotYawNormal.Margin = setMargin(plotYawNormal.Margin, Hscale, Wscale);

            plotPitchNormal.Height = Hscale * plotPitchNormal.Height;
            plotPitchNormal.Width = Wscale * plotPitchNormal.Width;
            plotPitchNormal.Margin = setMargin(plotPitchNormal.Margin, Hscale, Wscale);
            //设置按钮的大小和位置
            btnNormlizeSpanReset.Height = Hscale*btnNormlizeSpanReset.Height;
            btnNormlizeSpanReset.Width = Wscale * btnNormlizeSpanReset.Width;
            btnNormlizeSpanReset.Margin = setMargin(btnNormlizeSpanReset.Margin, Hscale, Wscale);

            btnAnalyzeSpanReset.Height = Hscale*btnAnalyzeSpanReset.Height;
            btnAnalyzeSpanReset.Width = Wscale * btnAnalyzeSpanReset.Width;
            btnAnalyzeSpanReset.Margin = setMargin(btnAnalyzeSpanReset.Margin, Hscale, Wscale);

            btnNormlize.Height = Hscale*btnNormlize.Height;
            btnNormlize.Width = Wscale * btnNormlize.Width;
            btnNormlize.Margin = setMargin(btnNormlize.Margin, Hscale, Wscale);

            //设置文本的位置和大小
            label.Height = Hscale * label.Height;
            label.Width = Wscale * label.Width;
            label.FontSize = label.FontSize * Hscale;
            label.Margin = setMargin(label.Margin, Hscale, Wscale);

            label1.Height = Hscale * label1.Height;
            label1.Width = Wscale * label1.Width;
            label1.FontSize = label1.FontSize * Hscale;
            label1.Margin = setMargin(label1.Margin, Hscale, Wscale);

            label_Copy.Height = Hscale * label_Copy.Height;
            label_Copy.Width = Wscale * label_Copy.Width;
            label_Copy.FontSize = label_Copy.FontSize*Hscale;
            label_Copy.Margin = setMargin(label_Copy.Margin, Hscale, Wscale);

            label_Copy1.Height = Hscale * label_Copy1.Height;
            label_Copy1.Width = Wscale * label_Copy1.Width;
            label_Copy1.FontSize = label_Copy1.FontSize * Hscale;
            label_Copy1.Margin = setMargin(label_Copy1.Margin, Hscale, Wscale);

            label_Copy2.Height = Hscale * label_Copy2.Height;
            label_Copy2.Width = Wscale * label_Copy2.Width;
            label_Copy2.FontSize = label_Copy2.FontSize * Hscale;
            label_Copy2.Margin = setMargin(label_Copy2.Margin, Hscale, Wscale);

            label_Copy3.Height = Hscale * label_Copy3.Height;
            label_Copy3.Width = Wscale * label_Copy3.Width;
            label_Copy3.FontSize = label_Copy3.FontSize * Hscale;
            label_Copy3.Margin = setMargin(label_Copy3.Margin, Hscale, Wscale);

            label_Copy4.Height = Hscale * label_Copy4.Height;
            label_Copy4.Width = Wscale * label_Copy4.Width;
            label_Copy4.FontSize = label_Copy4.FontSize * Hscale;
            label_Copy4.Margin = setMargin(label_Copy4.Margin, Hscale, Wscale);

            label_Copy5.Height = Hscale * label_Copy5.Height;
            label_Copy5.Width = Wscale * label_Copy5.Width;
            label_Copy5.FontSize = label_Copy5.FontSize * Hscale;
            label_Copy5.Margin = setMargin(label_Copy5.Margin, Hscale, Wscale);

            label_Copy6.Height = Hscale * label_Copy6.Height;
            label_Copy6.Width = Wscale * label_Copy6.Width;
            label_Copy6.FontSize = label_Copy6.FontSize * Hscale;
            label_Copy6.Margin = setMargin(label_Copy6.Margin, Hscale, Wscale);

            label_Copy7.Height = Hscale * label_Copy7.Height;
            label_Copy7.Width = Wscale * label_Copy7.Width;
            label_Copy7.FontSize = label_Copy7.FontSize * Hscale;
            label_Copy7.Margin = setMargin(label_Copy7.Margin, Hscale, Wscale);

            pitchMaxTextBox.Height = Hscale * pitchMaxTextBox.Height;
            pitchMaxTextBox.Width = Wscale * pitchMaxTextBox.Width;
            pitchMaxTextBox.FontSize = pitchMaxTextBox.FontSize * Hscale;
            pitchMaxTextBox.Margin = setMargin(pitchMaxTextBox.Margin, Hscale, Wscale);

            pitchMinTextBox.Height = Hscale * pitchMinTextBox.Height;
            pitchMinTextBox.Width = Wscale * pitchMinTextBox.Width;
            pitchMinTextBox.FontSize = pitchMinTextBox.FontSize * Hscale;
            pitchMinTextBox.Margin = setMargin(pitchMinTextBox.Margin, Hscale, Wscale);

            yawMaxTextBox.Height = Hscale * yawMaxTextBox.Height;
            yawMaxTextBox.Width = Wscale * yawMaxTextBox.Width;
            yawMaxTextBox.FontSize = yawMaxTextBox.FontSize * Hscale;
            yawMaxTextBox.Margin = setMargin(yawMaxTextBox.Margin, Hscale, Wscale);

            yawMinTextBox.Height = Hscale * yawMinTextBox.Height;
            yawMinTextBox.Width = Wscale * yawMinTextBox.Width;
            yawMinTextBox.FontSize = yawMinTextBox.FontSize * Hscale;
            yawMinTextBox.Margin = setMargin(yawMinTextBox.Margin, Hscale, Wscale);

            rollMaxTextBox.Height = Hscale * rollMaxTextBox.Height;
            rollMaxTextBox.Width = Wscale * rollMaxTextBox.Width;
            rollMaxTextBox.FontSize = rollMaxTextBox.FontSize * Hscale;
            rollMaxTextBox.Margin = setMargin(rollMaxTextBox.Margin, Hscale, Wscale);

            rollMinTextBox.Height = Hscale * rollMinTextBox.Height;
            rollMinTextBox.Width = Wscale * rollMinTextBox.Width;
            rollMinTextBox.FontSize = rollMinTextBox.FontSize * Hscale;
            rollMinTextBox.Margin = setMargin(rollMinTextBox.Margin, Hscale, Wscale);

            tabPanel.Height = Hscale * tabPanel.Height;
            tabPanel.Width = Wscale * tabPanel.Width;
            tabPanel.Margin = setMargin(tabPanel.Margin, Hscale, Wscale);
            menu_1.Height = Hscale * menu_1.Height;
            menu_1.Width = Hscale * menu_1.Width;
            menu_1.FontSize = menu_1.FontSize * Hscale;
            menu_1.Margin = setMargin(menu_1.Margin, Hscale, Wscale);
            menu_2.Height = Hscale * menu_2.Height;
            menu_2.Width = Hscale * menu_2.Width;
            menu_2.FontSize = menu_2.FontSize * Hscale;
            menu_2.Margin = setMargin(menu_2.Margin, Hscale, Wscale);

            sliderTraceZoom.Height = Hscale * sliderTraceZoom.Height;
            sliderTraceZoom.Width = Wscale * sliderTraceZoom.Width;
            sliderTraceZoom.Margin = setMargin(sliderTraceZoom.Margin, Hscale, Wscale);
        }

        private Thickness setMargin(Thickness s,double Hscale,double Wscale)
        {
            Thickness result=new Thickness();
            result.Left = s.Left * Wscale;
            result.Right = s.Right * Wscale;
            result.Bottom = s.Bottom * Hscale;
            result.Top = s.Top * Hscale;
            return result;
        }

        /// <summary>
        /// 清除工作空间的所有数据，包括每个数据图，所有文本框，所有最值标注点，所有区间红线，高亮轨迹等
        /// </summary>
        private void ClearWorkSpace()
        {
            flightBeanList.Clear();
            indexDict.Clear();
            normalizedFlightBeanList = null;
            analyzeSpan.Reset();
            normalizeSpan.Reset();
            clearHLight();
            clearanalysisTextBox();
            plotPitch.Children.RemoveAll(typeof(LineGraph));
            plotYaw.Children.RemoveAll(typeof(LineGraph));
            plotRoll.Children.RemoveAll(typeof(LineGraph));
            plotPitchNormal.Children.RemoveAll(typeof(LineGraph));
            plotYawNormal.Children.RemoveAll(typeof(LineGraph));
            plotRollNormal.Children.RemoveAll(typeof(LineGraph));
            traceChartPlotter.Children.RemoveAll(typeof(LineGraph));
            btnNormlize.IsEnabled = false;
        }
    }
}
