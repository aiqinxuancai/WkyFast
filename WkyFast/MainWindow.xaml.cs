using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
using WkyFast.Window;
using WkyApiSharp.Service.Model;
using WkyApiSharp.Service;
using System.Threading;
using WkyApiSharp.Events.Account;
using System.Reactive.Linq;

namespace WkyFast
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static MainWindow Instance { get; set; }

        private CancellationTokenSource _tokenTaskListSource = new CancellationTokenSource();


        public MainWindow()
        {
            InitializeComponent();

            Instance = this;

            IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
            Win11Style.LoadWin11Style(hWnd);

            LoadMainTabView();

            EasyLogManager.Logger.Info("主界面初始化");

            //支持选中？
            //默认加载第一个？的面板？
        }

        ~MainWindow()
        {
            //MainNotifyIcon.Visibility = Visibility.Hidden;
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoginFunc();
        }

        public void LoadMainTabView()
        {
            ObservableCollection<MainTabItemModel> model = new ObservableCollection<MainTabItemModel>();
            model.Add(new MainTabItemModel() { Title = "下载列表", Type = MainTabItemModelType.DownloadList });
            model.Add(new MainTabItemModel() { Title = "订阅列表", Type = MainTabItemModelType.SubscriptionList });
            WkyMainTabView.ViewModel = model;
            WkyMainTabView.OnConfigSelected += WkyMainTabView_OnConfigSelected;
        }


        private void WkyMainTabView_OnConfigSelected(object sender, RoutedEventArgs e)
        {


            if (sender is WkyFast.View.WkyMainTabView)
            {
                MainTabItemModel model = ((WkyFast.View.WkyMainTabView)sender).LastSelectedConfig;


                if (model.Type == MainTabItemModelType.DownloadList)
                {
                    WkyTaskListView.Visibility = Visibility.Visible;
                    WkySubscriptionListView.Visibility = Visibility.Hidden;
                }
                else if (model.Type == MainTabItemModelType.SubscriptionList)
                {
                    WkyTaskListView.Visibility = Visibility.Hidden;
                    WkySubscriptionListView.Visibility = Visibility.Visible;
                }

            }

        }

        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            WindowAddTask.Show(this);
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
                        Debug.WriteLine("设备更新成功");
                        DeviceComboBox.ItemsSource = WkyApiManager.Instance.API.GetAllDevice();
                        var device = await WkyApiManager.Instance.SelectDevice();
                        DeviceComboBox.SelectedItem = device;

                        SubscriptionManager.Instance.User = WkyApiManager.Instance.API.UserInfo.UserId;
                        WkySubscriptionListView.ViewModel = SubscriptionManager.Instance.SubscriptionModel; //订阅列表绑定
                        WkyTaskListView.ViewModel = WkyApiManager.Instance.TaskList; //任务列表绑定

                        await WkyApiManager.Instance.API.LoginAllPeer();
                        SubscriptionManager.Instance.Restart();
                    }
                    else
                    {
                        Debug.WriteLine("设备更新失败");
                        await this.Dispatcher.Invoke(async () => {
                            await this.ShowMessageAsync("登录失败", "无法获取到玩客云设备");
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


                Debug.WriteLine("正在登录...");
                var controller = await this.ShowProgressAsync("正在登录", "...");
                controller.SetIndeterminate();
                await Task.Delay(1000);

                bool loginResult = await api.StartLogin();
                if (loginResult) 
                {
                    //TODO 登录chen
                    WkyApiManager.Instance.API = api;
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

            LoginDialogDelegate loginDialogDelegate = async delegate (WkyFast.Window.LoginDialog loginDialog,
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
            WkyFast.Window.LoginDialog loginDialog = new WkyFast.Window.LoginDialog(loginDialogDelegate,
                mail,
                password,
                !string.IsNullOrWhiteSpace(password),
                autoLogin);

            await this.ShowMetroDialogAsync(loginDialog);
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
                var controller = await this.ShowProgressAsync("正在登录", "...");
                controller.SetIndeterminate();
                await Task.Delay(1000);

                WkyApiManager.Instance.API = new WkyApiSharp.Service.WkyApi(email, password, WkyLoginDeviceType.PC);

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
                await ShowLoginAccount();
                return false;
                throw;
            }
            return false;

        }


        private async Task CloseCustomDialog(DependencyObject obj)
        {
            var dialog = ((DependencyObject)obj).TryFindParent<BaseMetroDialog>()!;
            if (dialog != null)
            {
                await this.HideMetroDialogAsync(dialog);
            }
        }

        public static Task HideVisibleDialogs(MetroWindow parent)
        {
            return Task.Run(async () =>
            {
                await parent.Dispatcher.Invoke(async () =>
                {
                    BaseMetroDialog dialogBeingShow = await parent.GetCurrentDialogAsync<BaseMetroDialog>();

                    while (dialogBeingShow != null)
                    {
                        await parent.HideMetroDialogAsync(dialogBeingShow);
                        dialogBeingShow = await parent.GetCurrentDialogAsync<BaseMetroDialog>();
                    }
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

        private void SubscriptionButton_Click(object sender, RoutedEventArgs e)
        {
            WindowAddSubscription.Show(this);
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
    }
}
