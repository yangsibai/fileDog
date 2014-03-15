using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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
        private readonly DispatcherTimer t1;
        private readonly DispatcherTimer t2;
        private readonly Process _redisProc = null;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = TaskConfig.Config;
            t1 = new DispatcherTimer();
            t1.Tick += BeginResolveURL;
            t1.Interval = TimeSpan.FromSeconds(5);

            t2 = new DispatcherTimer();
            t2.Tick += BeginDownloadFile;
            t2.Interval = TimeSpan.FromSeconds(5);

            var path = Path.Combine(Directory.GetCurrentDirectory(), "Tools", "Redis_64", "redis-server.exe");
            var startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                FileName = path,
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
                _messageScrollViewer.ScrollToEnd();
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
                _messageScrollViewer.ScrollToEnd();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        ///     点击开始按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShowMessage("start at:" + TaskConfig.Config.StartURL);

            Task.Factory.StartNew(() => WebResolver.ResolveUrl(TaskConfig.Config.StartURL)).ContinueWith(continuation =>
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
            _redisProc.Kill(); //退出redis
        }
    }
}