using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using me.sibo.fileDog.Service;

namespace me.sibo.fileDog
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private DispatcherTimer t1;
        private DispatcherTimer t2;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = TaskConfig.Config;
            t1 = new DispatcherTimer(TimeSpan.FromSeconds(5), DispatcherPriority.Normal, Tick1, Dispatcher);
            t2 = new DispatcherTimer(TimeSpan.FromSeconds(10), DispatcherPriority.Normal, Tick2, Dispatcher);
        }

        private void Tick1(object sender, EventArgs e)
        {
            MessageTextBlock.Text += "到了该执行的时候了\n";
            var worker = new BackgroundWorker();
            worker.DoWork += (sender1, args) => WebResolver.ResolverUrl();
            worker.RunWorkerCompleted += (sender2, args2) => { MessageTextBlock.Text += "complete one url"; };
        }


        private void Tick2(object sender, EventArgs e)
        {
            MessageTextBlock.Text += "到了该执行download的时候了\n";
            var worker = new BackgroundWorker();
            worker.DoWork += (sen1, arg1) => WebResolver.DownloadFile();
            worker.RunWorkerCompleted += (sen2, arg2) => { MessageTextBlock.Text += "downloaded one url"; };
        }

        /// <summary>
        ///     点击开始按钮
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
                t1.Start();
                t2.Start();
            };
            worker.RunWorkerAsync();
        }

        /// <summary>
        ///     点击结束按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            t1.Stop();
            t2.Stop();
        }
    }
}