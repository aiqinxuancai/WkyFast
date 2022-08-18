
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
using WkyApiSharp.Service.Model;
using WkyApiSharp.Service;
using System.Threading;
using WkyApiSharp.Events.Account;
using System.Reactive.Linq;
using Wpf.Ui.Mvvm.Contracts;

namespace WkyFast
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; set; }

        private CancellationTokenSource _tokenTaskListSource = new CancellationTokenSource();


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

            await LoginFunc();
        }

        /// <summary>
        /// 重新登录
        /// </summary>
        public async void ReLoginFunc()
        {
            //移除账号相关信息
            WkyUserManager.Instance.Clear();
            await LoginFunc();
        }

        private void WkyMainTabView_OnConfigSelected(object sender, RoutedEventArgs e)
        {


            if (sender is WkyFast.View.WkyMainTabView)
            {
                MainTabItemModel model = ((WkyFast.View.WkyMainTabView)sender).LastSelectedConfig;


                if (model.Type == MainTabItemModelType.DownloadList)
                {
                    //WkyTaskListView.Visibility = Visibility.Visible;
                    //WkySubscriptionListView.Visibility = Visibility.Hidden;
                }
                else if (model.Type == MainTabItemModelType.SubscriptionList)
                {
                    //WkyTaskListView.Visibility = Visibility.Hidden;
                    //WkySubscriptionListView.Visibility = Visibility.Visible;
                }

            }

        }


        /// <summary>
        /// 登录确认成功时调用
        /// </summary>
        /// <returns></returns>
        private async Task OnLoginSuccess()
        {
            int runCount = 0;

            //注册通知获取数据
            WkyApiManager.Instance.API?.EventReceived
                .OfType<UpdateDeviceResultEvent>()
                .Subscribe(async r =>
                {
                    Debug.WriteLine("设备更新完毕");
                    if (r.IsSuccess)
                    {
                        //TODO 选中并设备
                        
                        DeviceComboBox.ItemsSource = WkyApiManager.Instance.API.GetAllDevice();
                        var device = await WkyApiManager.Instance.SelectDevice();
                        DeviceComboBox.SelectedItem = device;

                        Debug.WriteLine($"设备更新成功 选中{device?.PeerId}");

                        SubscriptionManager.Instance.User = WkyApiManager.Instance.API.UserInfo.UserId;

                        //TODO

                        await WkyApiManager.Instance.API.LoginAllPeer();
                        SubscriptionManager.Instance.Restart();

                        if (device == null)
                        {
                            RootSnackbar.Show("未发现玩客云设备", "请在手机端先添加设备后再使用WkyFast");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("设备更新失败");
                        await this.Dispatcher.Invoke(async () => {
                            //await this.ShowMessageAsync("登录失败", "无法获取到玩客云设备");

                            RootSnackbar.Show("需重新登录", "设备获取失败");

                            await ShowLoginAccount();
                        });
                    }
                });
        }


        private async Task LoginFunc()
        {
            //登录过程
            WkyUserManager.Instance.LoadPasswrod(out var mail, out var password, out var autoLogin);
            //先使用seesion
            if (autoLogin && !string.IsNullOrWhiteSpace(mail) && !string.IsNullOrWhiteSpace(password))
            {
                var api = new WkyApiSharp.Service.WkyApi(mail, password, WkyLoginDeviceType.PC);
                WkyApiManager.Instance.API = api;

                //api.EventReceived
                //    .OfType<LoginResultEvent>()
                //    .Subscribe(async r =>
                //    {
                //        if (r.IsSuccess)
                //        {
                //            Console.WriteLine("登录成功");
                //        }
                //    });

                Debug.WriteLine("正在登录...");
                //var controller = await this.ShowProgressAsync("正在登录", "...");
                //controller.SetIndeterminate();
                WkyLoginDialog.ShowLoading(true); 

                await Task.Delay(1000);

                bool loginResult = await api.StartLogin();
                if (loginResult) 
                {
                    //TODO 登录chen
                    
                    await OnLoginSuccess();
                    await HideVisibleDialogs(this);
                }
                else
                {
                    await HideVisibleDialogs(this);
                    await ShowLoginAccount();
                }
            }
            else
            {
                //未设置账号，弹出登录
                await ShowLoginAccount();
            }
        }

        private async Task ShowLoginAccount()
        {
            WkyUserManager.Instance.LoadPasswrod(out var mail, out var password, out var autoLogin);

            LoginDialogDelegate loginDialogDelegate = async delegate (WkyFast.Dialogs.LoginDialog loginDialog,
                LoginDialogTapType type,
                string email,
                string password,
                bool savePassword,
                bool autoLogin) {
                    if (type == LoginDialogTapType.Login)
                    {
                        await AutoLoginWithAccount(email, password, savePassword, autoLogin);
                    }
                };

            //打开登录弹窗
            WkyLoginDialog.UpdateDefaltData(loginDialogDelegate,
                mail,
                password,
                !string.IsNullOrWhiteSpace(password),
                autoLogin);

            WkyLoginDialog.Visibility = Visibility.Visible;
            WkyLoginDialog.ShowLoading(false);

            
        }



        /// <summary>
        /// 输入完账号密码的登录流程
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="savePassword"></param>
        /// <param name="autoLogin"></param>
        /// <returns></returns>
        private async Task<bool> AutoLoginWithAccount(string email, string password, bool savePassword, bool autoLogin)
        {
            try
            {
                Debug.WriteLine("正在登录...");
                WkyLoginDialog.ShowLoading(true);
                await Task.Delay(1000);

                WkyApiManager.Instance.API = new WkyApi(email, password, WkyLoginDeviceType.PC);

                var loginResult = await WkyApiManager.Instance.API.StartLogin();

                if (loginResult)
                {
                    Debug.WriteLine("登录成功");
                    if (savePassword)
                    {
                        WkyUserManager.Instance.SavePassword(email, password, autoLogin);
                    }
                    else
                    {
                        WkyUserManager.Instance.SavePassword(email, "", autoLogin);
                    }

                    await OnLoginSuccess();
                    await HideVisibleDialogs(this);
                    return true;
                }
                else
                {
                    Debug.WriteLine("登录失败 需要重新输入账号密码");
                    await HideVisibleDialogs(this);
                    await ShowLoginAccount();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("登录错误 " + ex.Message);
                RootSnackbar.Show("登录失败", ex.Message);
                await ShowLoginAccount();
                return false;
                throw;
            }
            return false;

        }

        public Task HideVisibleDialogs(Window parent)
        {
            return Task.Run(async () =>
            {
                await parent.Dispatcher.Invoke(async () =>
                {
                    WkyLoginDialog.Visibility = Visibility.Collapsed;
                });
            });
        }


        private async void DeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WkyDevice device = DeviceComboBox.SelectedItem as WkyDevice;
            if (device != null)
            {
                AppConfig.ConfigData.LastDeviceId = device.DeviceId;
                var selected = await WkyApiManager.Instance.SelectDevice();
                //DeviceComboBox.SelectedItem = device;
            }
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
    }
}
