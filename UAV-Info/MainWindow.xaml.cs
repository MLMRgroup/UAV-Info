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

namespace UAV_Info
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
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
            VerticalLine benchmarkLine = new VerticalLine();
            var transform = plotPitch.Transform;
            var mouseScreenPosition = Mouse.GetPosition(plotPitch.CentralGrid);
            var mousePositionInData = mouseScreenPosition.ScreenToViewport(transform);

            SolidColorBrush redBrush = new SolidColorBrush(System.Windows.Media.Colors.Red);
            benchmarkLine.Stroke = redBrush;
            benchmarkLine.Value = mousePositionInData.X;
        }

        private void importAngleData(object sender, RoutedEventArgs args) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true) {
               
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
