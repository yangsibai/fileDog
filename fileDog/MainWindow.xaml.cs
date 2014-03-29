using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
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
    public partial class MainWindow
    {
        private readonly Paragraph _paragraph;

        private readonly DispatcherTimer _taskStatusTimer;
        private readonly DispatcherTimer _taskTimer;
        private Process _redisProc;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = TaskConfig.GetInstance();

            _paragraph = new Paragraph();
            rtb.Document = new FlowDocument(_paragraph);

            _taskTimer = new DispatcherTimer();
            _taskTimer.Tick += BeginTask;
            _taskTimer.Interval = TimeSpan.FromSeconds(2);

            _taskStatusTimer = new DispatcherTimer();
            _taskStatusTimer.Tick += DisplayTaskStatus;
            _taskStatusTimer.Interval = TimeSpan.FromSeconds(3);

            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        /// <summary>
        /// 捕获未处理的异常
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ShowMessage(MessageType.Warn, e.Exception.Message);
        }

        /// <summary>
        ///     开始任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BeginTask(object sender, EventArgs e)
        {
            while (NetScheduler.Borrow())
            {
                string url;
                try
                {
                    url = Redis.PopFileUrl();
                    if (!string.IsNullOrEmpty(url))
                    {
                        Task.Factory.StartNew(() => WebResolver.DownloadFile(url))
                            .ContinueWith(continu =>
                            {
                                NetScheduler.Return();
                                ShowMessage(continu.Result);
                            }, TaskScheduler.FromCurrentSynchronizationContext());
                        continue;
                    }
                }
                catch
                {
                    break;
                }

                try
                {
                    url = Redis.PopUrl();
                    if (!string.IsNullOrEmpty(url))
                    {
                        Task.Factory.StartNew(() => WebResolver.ResolveUrl(url)).ContinueWith(task =>
                        {
                            NetScheduler.Return();
                            task.Result.Start();
                            task.Result.ContinueWith(continu => ShowMessage(continu.Result));
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                        continue;
                    }
                }
                catch
                {
                    break;
                }

                ShowMessage(MessageType.Warn, "没有文章信息也没有文件地址，任务停止");
                StopTask();
                break;
            }
        }

        /// <summary>
        ///     显示任务状态信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayTaskStatus(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => Redis.GetTaskStatus()).ContinueWith(cont =>
            {
                TaskInfo taskInfo = cont.Result;
                TaskStatusTransition.Content =
                    string.Format(
                        "url:{0}    checkedUrl:{1}    fileUrl:{2}    fileChecked:{3}    fileDownloaded:{4}    net:{5}",
                        taskInfo.UrlCount, taskInfo.UrlCheckedCount, taskInfo.FileUrlCount,
                        taskInfo.FileCheckedCount,
                        taskInfo.DownloadFileCount, NetScheduler.NetCount);
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

            Task.Factory.StartNew(() =>
            {
                string redisPath = Path.Combine(Directory.GetCurrentDirectory(), "Source", "Redis_64",
                    "redis-server.exe");
                var startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    FileName = redisPath,
                    WindowStyle = ProcessWindowStyle.Hidden,
                };
                _redisProc = Process.Start(startInfo);
            }).ContinueWith(task => WebResolver.ResolveUrl(TaskConfig.GetInstance().StartURL)).ContinueWith(task =>
            {
                task.Result.Start();
                task.Result.ContinueWith(cont =>
                {
                    ShowMessage(cont.Result);
                    _taskTimer.Start();
                    _taskStatusTimer.Start();
                });
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        ///     点击结束按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            StopTask();
        }

        /// <summary>
        /// stop task
        /// </summary>
        private void StopTask()
        {
            _taskTimer.Stop();
            _taskStatusTimer.Stop();
            NetScheduler.Reset();
            ShowMessage(MessageType.Infomation, "停止任务");
        }

        /// <summary>
        ///     显示结果信息
        /// </summary>
        /// <param name="myResult"></param>
        private void ShowMessage(MyResult myResult)
        {
            ShowMessage(myResult.Success ? MessageType.Success : MessageType.Warn, myResult.Message);
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
            }

            string message = DateTime.Now.ToString("MM-dd HH:mm:ss => ") + String.Join("\t", messages);
            new Thread(() => Logger.AppendLog(message)).Start();
            if (_paragraph.Inlines.Count() >= 1000)
            {
                _paragraph.Inlines.Clear();
            }
            _paragraph.Inlines.Add(new Run(message)
            {
                Foreground = bru
            });
            _paragraph.Inlines.Add(new LineBreak());
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
                string json = JsonConvert.SerializeObject(TaskConfig.GetInstance());
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "Source", "config.json"), json);
                _redisProc.Kill(); //退出redis
            }
            catch (Exception exception)
            {
//                MessageBox.Show(exception.Message);
            }
        }

        /// <summary>
        ///     清理所有数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Clear(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(Redis.FlushDb)
                .ContinueWith(cont => ShowMessage(MessageType.Infomation, "Clear All Data"),
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        ///     设置代理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Flyout.IsOpen = !Flyout.IsOpen;
        }
    }
}