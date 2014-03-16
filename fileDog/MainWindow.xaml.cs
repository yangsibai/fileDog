using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using me.sibo.fileDog.Model;
using me.sibo.fileDog.Service;
using Newtonsoft.Json;

namespace me.sibo.fileDog
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly DispatcherTimer t1;
        private readonly DispatcherTimer t2;
        private readonly Process _redisProc = null;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = TaskConfig.GetInstance();

            t1 = new DispatcherTimer();
            t1.Tick += BeginResolveURL;
            t1.Interval = TimeSpan.FromSeconds(3);

            t2 = new DispatcherTimer();
            t2.Tick += BeginDownloadFile;
            t2.Interval = TimeSpan.FromSeconds(2);

            var redisPath = Path.Combine(Directory.GetCurrentDirectory(), "Source", "Redis_64", "redis-server.exe");
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = redisPath,
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            _redisProc = Process.Start(startInfo);
        }

        /// <summary>
        ///     解析网页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BeginResolveURL(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => WebResolver.ResolveUrl()).ContinueWith(cont =>
            {
                ShowMessage(cont.Result.Message);
                MessageScrollViewer.ScrollToEnd();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        ///     开始下载文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BeginDownloadFile(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => WebResolver.DownloadFile()).ContinueWith(cont =>
            {
                ShowMessage(cont.Result.Message);
                MessageScrollViewer.ScrollToEnd();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        ///     点击开始按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShowMessage("start at:" + TaskConfig.GetInstance().StartURL);

            Task.Factory.StartNew(() => WebResolver.ResolveUrl(TaskConfig.GetInstance().StartURL)).ContinueWith(continuation =>
            {
                ShowMessage(continuation.Result.Message);
                t1.Start();
                t2.Start();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        ///     点击结束按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShowMessage("停止任务");
            t1.Stop();
            t2.Stop();
        }

        /// <summary>
        ///     显示信息
        /// </summary>
        /// <param name="messages"></param>
        private void ShowMessage(params string[] messages)
        {
            DateTime time = DateTime.Now;
            string msg = time.ToString("M-d HH:mm => ") + String.Join("\t", messages);
            MessageTextBlock.Text += msg + "\n";
        }

        /// <summary>
        /// 关闭程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            try
            {
                _redisProc.Kill(); //退出redis
            }
            catch (Exception exception)
            {
//                MessageBox.Show(exception.Message);
            }
        }
    }
}