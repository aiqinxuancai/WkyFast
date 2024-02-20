
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using WkyFast.Service;
using WkyFast.Utils;

namespace WkyFast.Dialogs
{
    /// <summary>
    /// WindowAddTask.xaml 的交互逻辑
    /// </summary>
    public partial class WindowAddSubscription : Window
    {
        public WindowAddSubscription()
        {
            InitializeComponent();
            IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
            Win11Style.LoadWin11Style(hWnd);
            LoadDefaultPathSelected();
        }

        public static void Show(Window owner)
        {
            WindowAddSubscription dialog = new WindowAddSubscription();
            dialog.Owner = owner;
            dialog.ShowDialog();
        }

        private void LoadDefaultPathSelected()
        {
            try
            {
                if (AppConfig.Instance.ConfigData.AddTaskSavePathDict.TryGetValue(AppConfig.Instance.ConfigData.Aria2Rpc, out var path))
                {
                    this.TextBoxPath.Text = path;
                }
                else
                {
                    this.TextBoxPath.Text = "/downloads";
                }
            }
            catch (Exception ex)
            {
                EasyLogManager.Logger.Error(ex);
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
                        path = TextBoxPath.Text;
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
                            path = TextBoxPath.Text;
                        }
                        else
                        {
                            path = TextBoxPath.Text + (TextBoxPath.Text.EndsWith("/") ? "" : "/") + title;
                        }



                        SubscriptionManager.Instance.Add(url, path, regex, regexEnable, autoDir: autoDir);

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


        private void TextBoxPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            //当前选择的设备ID
            AppConfig.Instance.ConfigData.AddSubscriptionSavePathDict[AppConfig.Instance.ConfigData.Aria2Rpc] = TextBoxPath.Text;
            AppConfig.Instance.Save();
        }

    }
}
