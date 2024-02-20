
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WkyFast.Service;
using WkyFast.Utils;
using WkyFast.View.Model;
using WkyFast.Dialogs;
using System.Threading;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Controls;

namespace WkyFast
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; set; }

        private CancellationTokenSource _tokenTaskListSource = new CancellationTokenSource();

        private bool _needExit = false;

        public MainWindow()
        {
            InitializeComponent();
            Wpf.Ui.Appearance.Accent.ApplySystemAccent();
            Instance = this;
            IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
            Win11Style.LoadWin11Style(hWnd);
            EasyLogManager.Logger.Info("主界面初始化");
        }

        ~MainWindow()
        {

        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ActionVersion.CheckVersion();
            SubscriptionManager.Instance.OnSubscriptionProgressChanged += SubscriptionManager_OnSubscriptionProgressChanged;
            GAHelper.Instance.RequestPageView($"启动到主界面{ActionVersion.Version}");
           
            //await LoginFunc();
        }

        private void SubscriptionManager_OnSubscriptionProgressChanged(int now, int max)
        {

            this.Dispatcher.Invoke(new Action(() =>
            {
                if (max > 0 && now != max)
                {
                    //进程中
                    this.subscriptionProgressBar.Visibility = Visibility.Visible;
                    this.subscriptionProgressBar.Value = now;
                    this.subscriptionProgressBar.Maximum = max;
                }
                else
                {
                    this.subscriptionProgressBar.Visibility = Visibility.Hidden;
                    this.subscriptionProgressBar.Value = 0;
                    this.subscriptionProgressBar.Maximum = 1;
                }


            }));


        }

        private void MetroWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            //退出
            SubscriptionManager.Instance.Stop();
            _tokenTaskListSource.Cancel();
            System.Environment.Exit(System.Environment.ExitCode);
        }

        private void ButtonGithub_Click(object sender, RoutedEventArgs e)
        {
            BrowserHelper.OpenUrlBrowser("https://github.com/aiqinxuancai/WkyFast");
        }

        private void MainNotifyIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            if (this.Visibility == Visibility.Hidden)
            {
                this.Visibility = Visibility.Visible;
                this.Focus();
            }
            else
            {
                this.Visibility = Visibility.Hidden;
            }
        }

        private void NavigationItem_Home_Click(object sender, RoutedEventArgs e)
        {
            BrowserHelper.OpenUrlBrowser("https://github.com/aiqinxuancai/WkyFast");
        }

        private void HomeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BrowserHelper.OpenUrlBrowser("https://github.com/aiqinxuancai/WkyFast");
        }

        private void TitleBar_CloseClicked(object sender, RoutedEventArgs e)
        {
            //自行处理事件，改为最小化
        }

        private void TaskbarExitMenu_Click(object sender, RoutedEventArgs e)
        {
            _needExit = true;
            this.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_needExit)
            {
                e.Cancel = true;

                // 自己处理
                this.Hide();
                //弹出提示
                //MyNotifyIcon.
            }


        }
    }
}
