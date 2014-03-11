using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MahApps.Metro.Controls;

namespace fileDog
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = TaskConfig.Config;
            var t = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Normal, Tick, this.Dispatcher);
        }

        private void Tick(object sender, EventArgs e)
        {
            var dateTime = DateTime.Now;
            MessageTextBlock.Text += dateTime+"\n";
            var guid = Guid.NewGuid();
            TaskStatusTransition.Content = new TextBlock
            {
                Text = guid.ToString(),
                SnapsToDevicePixels = true
            };
        }

        /// <summary>
        /// 点击开始按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
//            TaskConfig config = (TaskConfig) this.FindResource("Config");
            MessageBox.Show("start url" + TaskConfig.Config.StartURL);
        }

        /// <summary>
        /// 点击结束按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
