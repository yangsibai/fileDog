using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
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
    public partial class MainWindow : MetroWindow
    {
        private readonly Paragraph _paragraph;

        private readonly DispatcherTimer _taskStatusTimer;
        private readonly DispatcherTimer _taskTimer;
        private Process _redisProc;
        private TaskInfo _taskInfo;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = TaskConfig.GetInstance();

            _paragraph = new Paragraph();
            rtb.Document = new FlowDocument(_paragraph);

            _taskTimer = new DispatcherTimer();
            _taskTimer.Tick += BeginTask;
            _taskTimer.Interval = TimeSpan.FromSeconds(1);

            _taskStatusTimer = new DispatcherTimer();
            _taskStatusTimer.Tick += DisplayTaskStatus;
            _taskStatusTimer.Interval = TimeSpan.FromSeconds(3);
        }

        /// <summary>
        ///     开始任务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BeginTask(object sender, EventArgs e)
        {
            if (_taskInfo != null && NetScheduler.Borrow())
            {
                if (_taskInfo.UrlCount == 0 && _taskInfo.FileUrlCount == 0)
                {
                    ShowMessage(MessageType.Warn, "没有文章信息也没有文件地址，任务停止");
                    StopTask();
                }
                if (_taskInfo.FileUrlCount>0)
                {
                    Task.Factory.StartNew(() => WebResolver.DownloadFile())
                        .ContinueWith(continu =>
                        {
                            NetScheduler.Return();
                            ShowMessage(continu.Result);
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                else
                {
                    Task.Factory.StartNew(() => WebResolver.ResolveUrl()).ContinueWith(task =>
                    {
                        NetScheduler.Return();
                        task.Result.Start();
                        task.Result.ContinueWith(continu => ShowMessage(continu.Result));
                    }, TaskScheduler.FromCurrentSynchronizationContext());
                }
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
                _taskInfo = cont.Result;
                TaskStatusTransition.Content =
                    string.Format(
                        "url:{0}    checkedUrl:{1}    fileUrl:{2}    fileChecked:{3}    fileDownloaded:{4}    net:{5}",
                        _taskInfo.UrlCount, _taskInfo.UrlCheckedCount, _taskInfo.FileUrlCount,
                        _taskInfo.FileCheckedCount,
                        _taskInfo.DownloadFileCount, NetScheduler.NetCount);
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

        private void StopTask()
        {
            _taskTimer.Stop();
            _taskStatusTimer.Stop();
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
                    ;
            }

            _paragraph.Inlines.Add(new Run(DateTime.Now.ToString("MM-dd HH:mm:ss => ") + String.Join("\t", messages))
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
        /// 设置代理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Flyout.IsOpen = !Flyout.IsOpen;
        }
    }
}