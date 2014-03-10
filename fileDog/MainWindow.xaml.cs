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
using MahApps.Metro.Controls;

namespace fileDog
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private TaskConfig config = new TaskConfig()
        {
            StartURL = "http://www.ireadhome.com"
        };

        public MainWindow()
        {
            this.DataContext = config;
            InitializeComponent();
        }

        /// <summary>
        /// 点击开始按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
//            TaskConfig config = (TaskConfig) this.FindResource("Config");
            MessageBox.Show("start url" + config.StartURL);
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
