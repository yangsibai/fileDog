using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using fileDog;
using MahApps.Metro.Controls;
using me.sibo.fileDog.Service;

namespace me.sibo.fileDog
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
            MessageTextBlock.Text += "start at:" + TaskConfig.Config.StartURL;

            var worker = new BackgroundWorker();
            worker.DoWork += (sender1, args) => WebResolver.ResolverUrl(TaskConfig.Config.StartURL);
            worker.RunWorkerCompleted += (sender2, args) =>
            {
                MessageTextBlock.Text += "resolve url complete";
            };
            worker.RunWorkerAsync();
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
