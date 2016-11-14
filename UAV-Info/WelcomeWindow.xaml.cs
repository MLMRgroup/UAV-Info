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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace UAV_Info
{
    /// <summary>
    /// WelcomeWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WelcomeWindow : Window
    {
        private DispatcherTimer _timer;
        private MainWindow _MainWindow = null;
        private double _waitTime = 1.0;
        private double _timeUnit = 5; //Millisecond
        public WelcomeWindow()
        {
            InitializeComponent();
            //创建主窗口对象
            _MainWindow = new MainWindow();
            this.Loaded += new RoutedEventHandler(WelcomeWindow_Loaded);
            this.Closing += WelcomeWindow_Closing;
            this.Closed += WelcomeWindow_Closed;
            this.Topmost = true;
        }

        private void WelcomeWindow_Loaded(object sender, EventArgs e)
        {
            this._timer = new DispatcherTimer();
            ProgressBar.Value = 0;
            this._timer.Interval = TimeSpan.FromMilliseconds(_timeUnit);
            this._timer.Tick += timer_Tick;
            this._timer.Start();
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            if (ProgressBar.Value >= 1)
            {
                this.Close();
            }
            ProgressBar.Value += 1 / (_waitTime * 1000 / _timeUnit);
        }

        private void WelcomeWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this._timer.Stop();
            this._MainWindow.Show();
        }

        private void WelcomeWindow_Closed(object sender, EventArgs e)
        {
        }
    }
}
