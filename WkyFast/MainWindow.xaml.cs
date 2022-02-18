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

            do
            {
                if (runCount == 3)
                {
                    //获取多次NowDevice失败跳出
                    break;
                }
                try
                {
                    //失败可以重试？
                    await SelectDevice();
                }
                catch (Exception ex)
                {
                    //出错
                    Debug.WriteLine(ex.Message);
                }
                runCount++;
            }
            while (WkyApiManager.Instance.NowDevice == null);

            //多次获取失败，重新开始登录
            if (runCount == 3)
            {
                await this.ShowMessageAsync("登录失败", "无法获取到玩客云设备");
                await ShowLoginAccount();
            }
            else
            {
                //test
                //SubscriptionManager.Instance.CheckSubscription();
                SubscriptionManager.Instance.User = WkyApiManager.Instance.WkyApi.UserInfo.UserId;

                WkySubscriptionListView.ViewModel = SubscriptionManager.Instance.SubscriptionModel;
            }

            
        }

        /// <summary>
        /// 更新UI上的
        /// </summary>
        /// <returns></returns>
        private async Task SelectDevice()
        {
            await WkyApiManager.Instance.UpdateDevice(); //获取peer

            DeviceComboBox.ItemsSource = WkyApiManager.Instance.DeviceList;

            var device = await WkyApiManager.Instance.SelectDevice();

            DeviceComboBox.SelectedItem = device;

            if (!string.IsNullOrWhiteSpace(device?.Peerid))
            {
                await WkyApiManager.Instance.WkyApi.RemoteDownloadLogin(device?.Peerid);

                WkyTaskListView.ViewModel = WkyApiManager.Instance.TaskList;

                _tokenTaskListSource = new CancellationTokenSource();
                Task.Run(async () =>
                {
                    await UpdateTaskFunc(_tokenTaskListSource.Token);
                }, _tokenTaskListSource.Token);
            }
            //刷新下载列表
        }


        private async Task UpdateTaskFunc(CancellationToken cancellationToken)
        {
            while (true) {
                if (cancellationToken.IsCancellationRequested)
                {
                    Debug.WriteLine("退出Task刷新");
                    break;
                }
                Debug.WriteLine("刷新Task列表");
                try
                {
                    await WkyApiManager.Instance.UpdateTask();
                }
                catch (Exception ex)
                {
                    EasyLogManager.Logger.Error(ex.ToString());
                }
                

                TaskHelper.Sleep(5 * 1000, 100, cancellationToken);
            }
        }


        private async Task LoginFunc()
        {
            //登录过程
            WkyUserManager.Instance.LoadPasswrod(out var mail, out var password, out var autoLogin);
            //先使用seesion
            if (autoLogin && !string.IsNullOrWhiteSpace(mail) && !string.IsNullOrWhiteSpace(password))
            {
                bool useSession = false;
                bool autoLoginSuccess = false;
                if (File.Exists("Session.json"))
                {
                    //检查日期
                    Debug.WriteLine("使用session");
                    var sessionContent = File.ReadAllText("Session.json");
                    try
                    {
                        WkyApiLoginResultModel loginResultModel = JsonConvert.DeserializeObject<WkyApiLoginResultModel>(sessionContent);
                        var api = new WkyApiSharp.Service.WkyApi(sessionContent, "", "", WkyLoginDeviceType.PC);
                        var listPeer = await api.ListPeer(); //检查session是否可用
                        if (listPeer.Rtn == 0)
                        {
                            Debug.WriteLine("Session可用");
                            useSession = true;
                            WkyApiManager.Instance.WkyApi = api;
                            Debug.WriteLine("session登录完成");
                            await OnLoginSuccess();
                        }
                        else
                        {
                            Debug.WriteLine($"session登录失败：{listPeer.Msg}");
                            //验证失败，尝试走自动登录流程
                            autoLoginSuccess = await AutoLoginWithAccount(mail, password, autoLogin, autoLogin);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Session登录错误：" + ex.Message);
                        await ShowLoginAccount();
                        throw;
                    }
                }
                if (!useSession && !autoLoginSuccess) //判断自动登录是否成功
                {
                    await ShowLoginAccount();
                }
            }
            else
            {
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

        private async Task<bool> AutoLoginWithAccount(string email, string password, bool savePassword, bool autoLogin)
        {
            try
            {
                Debug.WriteLine("正在登录...");
                var controller = await this.ShowProgressAsync("正在登录", "...");
                controller.SetIndeterminate();
                await Task.Delay(1000);

                WkyApiManager.Instance.WkyApi = new WkyApiSharp.Service.WkyApi(email, password, WkyLoginDeviceType.PC);

                var loginResult = await WkyApiManager.Instance.WkyApi.Login();

                if (loginResult)
                {
                    await HideVisibleDialogs(this);
                    Debug.WriteLine("登录成功");

                    var sessionContent = WkyApiManager.Instance.WkyApi.GetSessionContent();
                    File.WriteAllText("Session.json", sessionContent);
                    if (savePassword)
                    {
                        WkyUserManager.Instance.SavePassword(email, password, autoLogin);
                    }
                    else
                    {
                        WkyUserManager.Instance.SavePassword(email, "", autoLogin);
                    }

                    await OnLoginSuccess();
                    return true;
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


        private void DeviceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WkyApiSharp.Service.Model.ListPeer.Device? device = DeviceComboBox.SelectedItem as WkyApiSharp.Service.Model.ListPeer.Device;
            if (device != null)
            {
                AppConfig.ConfigData.LastDeviceId = device.DeviceId;
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
    }
}
