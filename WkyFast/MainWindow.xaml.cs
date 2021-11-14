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

namespace WkyFast
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {    

        public MainWindow()
        {
            InitializeComponent();

            IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
            Win11Style.LoadWin11Style(hWnd);

            LoadMainTabView();

            //支持选中？
            //默认加载第一个？的面板？
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoginFunc();
        }

        public void LoadMainTabView()
        {
            ObservableCollection<MainTabItemModel> model = new ObservableCollection<MainTabItemModel>();
            model.Add(new MainTabItemModel() { Title = "下载列表" });
            model.Add(new MainTabItemModel() { Title = "订阅列表" });
            WkyMainTabView.ViewModel = model;
            WkyMainTabView.OnConfigSelected += WkyMainTabView_OnConfigSelected;
        }


        private void WkyMainTabView_OnConfigSelected(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
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
            while (WkyApiManager.NowDevice == null);

            //多次获取失败，重新开始登录
            if (runCount == 3)
            {
                await this.ShowMessageAsync("登录失败", "无法获取到玩客云设备");
                await ShowLoginAccount();
            }

            
        }

        /// <summary>
        /// 更新UI上的
        /// </summary>
        /// <returns></returns>
        private async Task SelectDevice()
        {
            await WkyApiManager.UpdateDevice(); //获取peer

            DeviceComboBox.ItemsSource = WkyApiManager.DeviceList;

            var device = await WkyApiManager.SelectDevice();

            DeviceComboBox.SelectedItem = device;

            if (!string.IsNullOrWhiteSpace(device?.Peerid))
            {
                await WkyApiManager.WkyApi.RemoteDownloadLogin(device?.Peerid);
                var remoteDownloadListResult = await WkyApiManager.WkyApi.RemoteDownloadList(device?.Peerid);
                var obList = new ObservableCollection<WkyApiSharp.Service.Model.RemoteDownloadList.Task>(remoteDownloadListResult.Tasks.ToList());
                WkyTaskListView.ViewModel = obList;
            }
            //刷新下载列表
        }

        private async Task LoginFunc()
        {
            //登录过程
            WkyUserManager.LoadPasswrod(out var mail, out var password, out var autoLogin);
            //先使用seesion
            if (autoLogin && !string.IsNullOrWhiteSpace(mail) && !string.IsNullOrWhiteSpace(password))
            {
                bool useSession = false;
                if (File.Exists("Session.json"))
                {
                    //检查日期
                    Debug.WriteLine("使用session");
                    var sessionContent = File.ReadAllText("Session.json");
                    try
                    {
                        WkyApiLoginResultModel loginResultModel = JsonConvert.DeserializeObject<WkyApiLoginResultModel>(sessionContent);
                        var api = new WkyApiSharp.Service.WkyApi(sessionContent);
                        var listPeer = await api.ListPeer(); //检查session是否可用
                        if (listPeer.Rtn == 0)
                        {
                            Debug.WriteLine("Session可用");
                            useSession = true;
                            WkyApiManager.WkyApi = api;
                            Debug.WriteLine("session登录完成");
                            await OnLoginSuccess();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Session登录错误 " + ex.Message);
                        await ShowLoginAccount();
                        throw;
                    }
                }
                if (!useSession) //没有使用
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
            WkyUserManager.LoadPasswrod(out var mail, out var password, out var autoLogin);

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

        private async Task AutoLoginWithAccount(string email, string password, bool savePassword, bool autoLogin)
        {
            try
            {
                var controller = await this.ShowProgressAsync("正在登录", "...");
                controller.SetIndeterminate();
                await Task.Delay(1000);

                WkyApiManager.WkyApi = new WkyApiSharp.Service.WkyApi(email, password);

                var loginResult = await WkyApiManager.WkyApi.Login();

                if (loginResult)
                {
                    await HideVisibleDialogs(this);
                    Debug.WriteLine("登录成功");

                    var sessionContent = WkyApiManager.WkyApi.GetSessionContent();
                    File.WriteAllText("Session.json", sessionContent);
                    if (savePassword)
                    {
                        WkyUserManager.SavePassword(email, password, autoLogin);
                    }
                    else
                    {
                        WkyUserManager.SavePassword(email, "", autoLogin);
                    }

                    await OnLoginSuccess();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("登录错误 " + ex.Message);
                await ShowLoginAccount();
                throw;
            }
            

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
    }
}
