
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;
using WkyApiSharp.Service.Model;
using WkyFast.Service;
using WkyFast.Utils;

namespace WkyFast.Dialogs
{
    /// <summary>
    /// WindowAddTask.xaml 的交互逻辑
    /// </summary>
    public partial class WindowAddSubscription : Window
    {
        private WkyDevice? _device;

        public WindowAddSubscription()
        {
            InitializeComponent();
            IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
            Win11Style.LoadWin11Style(hWnd);

            _device = WkyApiManager.Instance.NowDevice;

            this.ComboBoxPartition.ItemsSource = _device?.Partitions;
            LoadDefaultPartitionSelected();
            LoadDefaultPathSelected();
        }

        public static void Show(Window owner)
        {
            WindowAddSubscription dialog = new WindowAddSubscription();
            dialog.Owner = owner;
            dialog.ShowDialog();
        }


        private void LoadDefaultPartitionSelected()
        {
            if (AppConfig.Instance.ConfigData.AddSubscriptionSavePartitionDict.TryGetValue(_device.DeviceId, out var partitionpath))
            {
                //寻找
                var p = _device.Partitions.FirstOrDefault(a => a.Partition.Path == partitionpath);

                if (p != null)
                {
                    this.ComboBoxPartition.SelectedIndex = _device.Partitions.IndexOf(p);
                }
                else
                {
                    this.ComboBoxPartition.SelectedIndex = 0;
                }
            }
            else
            {
                this.ComboBoxPartition.SelectedIndex = 0;
            }

        }
        private void LoadDefaultPathSelected()
        {
            WkyPartition wkyPartition = (WkyPartition)ComboBoxPartition.SelectedItem;

            if (AppConfig.Instance.ConfigData.AddSubscriptionSavePathDict.TryGetValue(wkyPartition.Partition.Path, out var path))
            {
                this.TextBoxPath.Text = path;
            }
            else
            {
                this.TextBoxPath.Text = "/onecloud/tddownload";
            }
        }


        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {

            //if (string.IsNullOrWhiteSpace(AppConfig.Instance.ConfigData.LastAddSubscriptionPath))
            //{
            //    TextBoxPath.Text = "/onecloud/tddownload";
            //}
            //else
            //{
            //    //TODO
            //    TextBoxPath.Text = AppConfig.Instance.ConfigData.LastAddSubscriptionPath;
            //}

        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TextBoxPath.Text.StartsWith("/"))
            {
                MainWindow.Instance.ShowSnackbar("添加失败", $"路径需要用/开头");
                return;
            }

            ConfirmButton.IsEnabled = false;
            //TODO 支持选择设备和磁盘？？
            try
            {
                //var progressView = await this.ShowProgressAsync("请稍后", "正在检查订阅...");
                WkyPartition wkyPartition = (WkyPartition)ComboBoxPartition.SelectedItem;

                await Task.Run(() => {
                    string url = string.Empty;
                    string regex = string.Empty;
                    bool regexEnable = false;
                    string path = string.Empty;
                    bool autoDir = false;

                    this.Dispatcher.Invoke(() =>
                    {
                        url = UrlTextBox.Text;
                        regex = RegexTextBox.Text;
                        regexEnable = RegexCheckBox.IsChecked == true ? true : false;
                        path = wkyPartition.Partition.Path + TextBoxPath.Text;
                        autoDir = AutoDirSwitch.IsChecked == true ? true : false;
                    });

                    try
                    {
                        Uri uri = new Uri(url);
                    } 
                    catch (Exception ex)
                    {
                        //progressView.CloseAsync();
                        //this.ShowMessageAsync("Url不合法", ex.ToString());
                        //return;
                        EasyLogManager.Logger.Error(ex);
                    }
                    
                    string title = SubscriptionManager.Instance.GetSubscriptionTitle(url);

                    this.Dispatcher.Invoke(() =>
                    {
                        if (string.IsNullOrEmpty(title))
                        {
                            path = wkyPartition.Partition.Path + TextBoxPath.Text;
                        }
                        else
                        {
                            path = wkyPartition.Partition.Path + TextBoxPath.Text + (TextBoxPath.Text.EndsWith("/") ? "" : "/") + title;
                        }



                        SubscriptionManager.Instance.Add(url, _device, path, regex, regexEnable, autoDir: autoDir);

                        EasyLogManager.Logger.Info($"订阅已添加：{title} {url}");

                        MainWindow.Instance.ShowSnackbar("添加成功", $"已添加订阅{title}", Wpf.Ui.Common.SymbolRegular.AddCircle24);
                        //AppConfig.Instance.ConfigData.LastAddSubscriptionPath = TextBoxPath.Text;
                    });

                });

                //await progressView.CloseAsync();

                this.Close();
            }
            catch (Exception ex)
            {
                //await this.ShowMessageAsync("添加异常，请重试", ex.ToString());
                EasyLogManager.Logger.Error(ex);
            }
            ConfirmButton.IsEnabled = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ComboBoxPartition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


            WkyPartition wkyPartition = (WkyPartition)((ComboBox)e.Source).SelectedItem;
            //当前选择的设备ID
            AppConfig.Instance.ConfigData.AddSubscriptionSavePartitionDict[AppConfig.Instance.ConfigData.LastDeviceId] = wkyPartition.Partition.Path;
            AppConfig.Instance.Save();
        }

        private void TextBoxPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            WkyPartition wkyPartition = (WkyPartition)ComboBoxPartition.SelectedItem;
            //当前选择的设备ID
            AppConfig.Instance.ConfigData.AddSubscriptionSavePathDict[wkyPartition.Partition.Path] = TextBoxPath.Text;
            AppConfig.Instance.Save();
        }

    }
}
