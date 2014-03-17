using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using me.sibo.fileDog.Model;
using me.sibo.fileDog.Service;

namespace me.sibo.fileDog
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly Paragraph _paragraph;
        private readonly Process _redisProc;
        private readonly DispatcherTimer t1;
        private readonly DispatcherTimer t2;
        private readonly DispatcherTimer t3;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = TaskConfig.GetInstance();

            _paragraph = new Paragraph();
            rtb.Document = new FlowDocument(_paragraph);

            t1 = new DispatcherTimer();
            t1.Tick += BeginResolveURL;
            t1.Interval = TimeSpan.FromSeconds(3);

            t2 = new DispatcherTimer();
            t2.Tick += BeginDownloadFile;
            t2.Interval = TimeSpan.FromSeconds(2);

            t3 = new DispatcherTimer();
            t3.Tick += GetTaskInfo;
            t3.Interval = TimeSpan.FromSeconds(3);

            string redisPath = Path.Combine(Directory.GetCurrentDirectory(), "Source", "Redis_64", "redis-server.exe");
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
            Task.Factory.StartNew(() => WebResolver.ResolveUrl())
                .ContinueWith(cont => ShowMessage(cont.Result), TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        ///     开始下载文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BeginDownloadFile(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => WebResolver.DownloadFile())
                .ContinueWith(cont => ShowMessage(cont.Result), TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        ///     获取task信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetTaskInfo(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => Redis.GeTaskInfo()).ContinueWith(cont =>
            {
                TaskInfo taskInfo = cont.Result;
                TaskStatusTransition.Content =
                    string.Format("url:{0}    checkedUrl:{1}    fileUrl:{2}    fileChecked:{3}    fileDownloaded:{4}",
                        taskInfo.UrlCount, taskInfo.UrlCheckedCount, taskInfo.FileUrlCount, taskInfo.FileCheckedCount,
                        taskInfo.DownloadFileCount);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        ///     点击开始按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShowMessage(MessageType.Infomation, "start at:", TaskConfig.GetInstance().StartURL);
            Task.Factory.StartNew(() => WebResolver.ResolveUrl(TaskConfig.GetInstance().StartURL))
                .ContinueWith(continuation =>
                {
                    ShowMessage(continuation.Result);
                    t1.Start();
                    t2.Start();
                    t3.Start();
                }, TaskScheduler.FromCurrentSynchronizationContext());
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
            t3.Stop();
            ShowMessage(MessageType.Infomation, "停止任务");
        }

        /// <summary>
        ///     显示结果信息
        /// </summary>
        /// <param name="result"></param>
        private void ShowMessage(Result result)
        {
            ShowMessage(result.Success ? MessageType.Success : MessageType.Warn, result.Message);
        }

        /// <summary>
        ///     显示信息
        /// </summary>
        /// <param name="messages"></param>
        private void ShowMessage(MessageType msgType, params string[] messages)
        {
            SolidColorBrush bru;
            switch (msgType)
            {
                case MessageType.Infomation:
                    bru = Brushes.CornflowerBlue;
                    break;
                case MessageType.Success:
                    bru = Brushes.Green;
                    break;
                case MessageType.Warn:
                    bru = Brushes.Red;
                    break;
                default:
                    bru = Brushes.DarkBlue;
                    break;
                    ;
            }

            _paragraph.Inlines.Add(new Run(DateTime.Now.ToString("MM-dd HH:mm:ss => ") + String.Join("\t", messages))
            {
                Foreground = bru
            });
            _paragraph.Inlines.Add(new LineBreak());


//            var tr = new TextRange(rtb.Document.ContentEnd, rtb.Document.ContentEnd)
//            {
//                Text =DateTime.Now.ToString("MM-dd HH:mm:ss => ") + String.Join("\t", messages)
//            };
//            tr.ApplyPropertyValue(TextElement.ForegroundProperty, bru);
//            rtb.AppendText("\n");
            MessageScrollViewer.ScrollToEnd();
        }

        /// <summary>
        ///     关闭程序
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