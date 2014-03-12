using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
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
        private DispatcherTimer t1;
        private DispatcherTimer t2;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = TaskConfig.Config;
            t1=new DispatcherTimer();
            t1.Tick += Tick1;
            t1.Interval = TimeSpan.FromSeconds(5);

            t2=new DispatcherTimer();
            t2.Tick += Tick2;
            t2.Interval = TimeSpan.FromSeconds(5);
        }

        private void Tick1(object sender, EventArgs e)
        {
            MessageTextBlock.Text += "到了该执行的时候了\n";

            var task = Task.Factory.StartNew(() => WebResolver.ResolverUrl()).ContinueWith(cont =>
            {
                MessageTextBlock.Text += cont.Result.Message + "\n";
                _messageScrollViewer.ScrollToEnd();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }


        private void Tick2(object sender, EventArgs e)
        {
            MessageTextBlock.Text += "到了该执行download的时候了\n";

            var task = Task.Factory.StartNew(() => WebResolver.DownloadFile()).ContinueWith(cont =>
            {
                MessageTextBlock.Text += cont.Result.Message + "\n";
                _messageScrollViewer.ScrollToEnd();
            },TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        ///     点击开始按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            MessageTextBlock.Text += "start at:" + TaskConfig.Config.StartURL+"\n";

            var task = Task.Factory.StartNew(() => WebResolver.ResolverUrl(TaskConfig.Config.StartURL));

            task.ContinueWith(continuation =>
            {
                MessageTextBlock.Text += continuation.Result.Message;
                _messageScrollViewer.ScrollToEnd();
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
            t1.Stop();
            t2.Stop();
        }
    }
}