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
using Microsoft.Research.DynamicDataDisplay.Charts;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.Charts.Navigation;
using System.Collections.ObjectModel;
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
            Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        double maxPitchIndex = 100;
        double maxYawIndex = 100;
        double maxRollIndex = 100;
        double minPitchIndex = 0;
        double minYawIndex = 0;
        double minRollIndex = 0;
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
            if (e.PropertyName == "Visible")
            {
                plotPitch.Viewport.Visible = new Rect(((Viewport2D)sender).Visible.X, minPitchIndex, ((Viewport2D)sender).Visible.Width, maxPitchIndex);
                plotYaw.Viewport.Visible = new Rect(((Viewport2D)sender).Visible.X, minYawIndex, ((Viewport2D)sender).Visible.Width, maxYawIndex);
                plotRoll.Viewport.Visible = new Rect(((Viewport2D)sender).Visible.X, minRollIndex, ((Viewport2D)sender).Visible.Width, maxRollIndex);
            }
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



    }
}
