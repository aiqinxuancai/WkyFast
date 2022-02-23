using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
using WkyFast.Service;
using WkyFast.Utils;

namespace WkyFast.Window
{
    /// <summary>
    /// WindowAddTask.xaml 的交互逻辑
    /// </summary>
    public partial class WindowAddSubscription : MetroWindow
    {
        public WindowAddSubscription()
        {
            InitializeComponent();
            IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
            Win11Style.LoadWin11Style(hWnd);
        }

        public static void Show(MetroWindow owner)
        {
            WindowAddSubscription dialog = new WindowAddSubscription();
            dialog.Owner = owner;
            dialog.ShowDialog();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(AppConfig.ConfigData.LastAddSubscriptionPath))
            {
                PathTextBox.Text = "/onecloud/tddownload";
            }
            else
            {
                //TODO
                PathTextBox.Text = AppConfig.ConfigData.LastAddSubscriptionPath;
            }
            
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmButton.IsEnabled = false;
            //TODO 支持选择设备和磁盘？？
            try
            {
                await Task.Run(() => {
                    string url = string.Empty;
                    string regex = string.Empty;
                    bool regexEnable = false;
                    string path = string.Empty;
                    this.Dispatcher.Invoke(() =>
                    {
                        url = UrlTextBox.Text;
                        regex = RegexTextBox.Text;
                        regexEnable = RegexCheckBox.IsChecked == true ? true : false;
                        path = PathTextBox.Text;
                        SubscriptionManager.Instance.Add(url, regex, path, regexEnable);

                        AppConfig.ConfigData.LastAddSubscriptionPath = path;

                    });

                    
                });
               
                this.Close();
            }
            catch (Exception ex)
            {
                await this.ShowMessageAsync("添加异常，请重试", ex.ToString());
            }
            ConfirmButton.IsEnabled = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


    }
}
