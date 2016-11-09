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
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        double maxPitchIndex = 5;
        double maxYawIndex = 400;
        double maxRollIndex = 2;
        double minPitchIndex = -5;
        double minYawIndex = 0;
        double minRollIndex = -2;
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
          plotPitch.Children.Add(normalizeSpan.LineA);
          plotPitch.Children.Add(normalizeSpan.LineB);
          plotPitch.Children.Remove(plotPitch.KeyboardNavigation);
          //plotPitch.Viewport.Restrictions.Add();
          plotYaw.Children.Remove(plotYaw.KeyboardNavigation);
          // plotYaw.DefaultContextMenu.Remove();
          plotRoll.Children.Remove(plotRoll.KeyboardNavigation);
          plotPitch.MouseDoubleClick += attiPlotMouseClick;

          // Add handler
          plotPitch.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(Viewport_PropertyChanged);
          plotYaw.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(Viewport_PropertyChanged);
          plotRoll.Viewport.PropertyChanged += new EventHandler<ExtendedPropertyChangedEventArgs>(Viewport_PropertyChanged);


        }

        private void OnClick_btnBenchmarkReset(object sender, RoutedEventArgs e)
        {
            normalizeSpan.Reset();
        }
        // Respond to changes
        void Viewport_PropertyChanged(object sender, ExtendedPropertyChangedEventArgs e)
        {
            /*if (e.PropertyName == "Visible")
            {
                plotPitch.Viewport.Visible = new Rect(((Viewport2D)sender).Visible.X, minPitchIndex, ((Viewport2D)sender).Visible.Width, maxPitchIndex);
                plotYaw.Viewport.Visible = new Rect(((Viewport2D)sender).Visible.X, minYawIndex, ((Viewport2D)sender).Visible.Width, maxYawIndex);
                plotRoll.Viewport.Visible = new Rect(((Viewport2D)sender).Visible.X, minRollIndex, ((Viewport2D)sender).Visible.Width, maxRollIndex);
            }*/
        }

        private void attiPlotMouseClick(object sender, MouseEventArgs e)
        {
            var transform = plotPitch.Transform;
            var mouseScreenPosition = Mouse.GetPosition(plotPitch.CentralGrid);
            var mousePositionInData = mouseScreenPosition.ScreenToViewport(transform);

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
                // 轨迹数据
            }
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
                plotRoll.Children.Add(lineG);
            }
            
        }

        private void animationTimer_Tick(object sender, EventArgs e)
        {
            Point pos = new Point(dateAxis_angle.ConvertToDouble(TimeUtils.strToDateTime(TimeUtils.toformatTime("160529163534"))), 0);
            Point zoomTo = pos.DataToScreen(plotPitch.Viewport.Transform);
            plotPitch.Viewport.Visible.Zoom(zoomTo, 3.0);
        }

    }
}
