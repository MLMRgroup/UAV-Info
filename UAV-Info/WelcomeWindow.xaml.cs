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
        //等待的时间度量，与实际时间不一定相符
        private double _waitTime = 1.0;
        //定时器的基本时间间隔，单位为Millisecond
        private double _timeUnit = 5; 
        public WelcomeWindow()
        {
            InitializeComponent();
            //创建主窗口对象
            _MainWindow = new MainWindow();
            //添加窗口事件
            this.Loaded += new RoutedEventHandler(WelcomeWindow_Loaded);
            this.Closing += WelcomeWindow_Closing;
            this.Closed += WelcomeWindow_Closed;
            //最前端显示
            this.Topmost = true;
        }
        //欢迎窗口载入
        private void WelcomeWindow_Loaded(object sender, EventArgs e)
        {
            //初始化定时器，初始化ProgressBar
            this._timer = new DispatcherTimer();
            ProgressBar.Value = 0;
            this._timer.Interval = TimeSpan.FromMilliseconds(_timeUnit);
            this._timer.Tick += timer_Tick;
            this._timer.Start();
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            //ProgressBar进度到1之后，则关闭本窗口  进而打开主窗口
            if (ProgressBar.Value >= 1)
            {
                this.Close();
            }
            else //否则按比例增加进度条的值
            {
                ProgressBar.Value += 1 / (_waitTime * 1000 / _timeUnit);
            }
           
        }

        private void WelcomeWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //关掉定时器，显示主窗口
            this._timer.Stop();
            this._MainWindow.Show();
        }

        private void WelcomeWindow_Closed(object sender, EventArgs e)
        {
        }
    }
}
