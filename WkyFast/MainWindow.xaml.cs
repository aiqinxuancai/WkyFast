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
            await ShowLoginAccount(true);
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
        /// 更新UI上的
        /// </summary>
        /// <returns></returns>
        private async Task SelectDevice()
        {
            await WkyApiManager.UpdateDevice();

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


        private async Task ShowLoginAccount(bool useSessionLogin)
        {
            //登录过程
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


            WkyUserManager.LoadPasswrod(out var mail, out var password, out var autoLogin);


            if (autoLogin && !string.IsNullOrWhiteSpace(mail) && !string.IsNullOrWhiteSpace(password))
            {
                //先使用seesion
                bool useSession = false;
                if (useSessionLogin && File.Exists("Session.json"))
                {
                    //检查日期
                    Debug.WriteLine("使用session");
                    var sessionContent = File.ReadAllText("Session.json");
                    WkyApiLoginResultModel loginResultModel = JsonConvert.DeserializeObject<WkyApiLoginResultModel>(sessionContent);

                    //检查session是否可用
                    var api = new WkyApiSharp.Service.WkyApi(sessionContent);
                    var listPeer = await api.ListPeer();

                    if (listPeer.Rtn == 0) //使用session登录
                    {
                        Debug.WriteLine("Session可用");
                        useSession = true;
                        WkyApiManager.WkyApi = api;
                        //var sn = listPeer.Result.Last().ResultClass.Devices.First().DeviceSn;
                        //var turn = await api.GetTurnServer(sn);
                        //Debug.WriteLine(turn.ToString());

                        await SelectDevice();

                        Debug.WriteLine("session登录完成");
                    }
                }

                if (!useSession) //没有使用
                {
                    await AutoLoginWithAccount(mail, password, true, autoLogin);
                }

            }
            else
            {
                //打开登录弹窗
                WkyFast.Window.LoginDialog loginDialog = new WkyFast.Window.LoginDialog(loginDialogDelegate,
                    mail,
                    password,
                    !string.IsNullOrWhiteSpace(password),
                    autoLogin);

                await this.ShowMetroDialogAsync(loginDialog);
            }
        }

        private async Task AutoLoginWithAccount(string email, string password, bool savePassword, bool autoLogin)
        {
            //TODO
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

                await SelectDevice();

                if (savePassword)
                {
                    WkyUserManager.SavePassword(email, password, autoLogin);
                }
                else
                {
                    WkyUserManager.SavePassword(email, "", autoLogin);
                }

                //GlobalNotification.Default.Post(GlobalNotificationType.NotificationLoginSuccess, resultSession);
                //EmailLabel.Content = email;
            }
            else
            {
                await HideVisibleDialogs(this);
                MessageDialogResult messageResult = await this.ShowMessageAsync("登录失败", "0.0");
                await ShowLoginAccount(false);
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
