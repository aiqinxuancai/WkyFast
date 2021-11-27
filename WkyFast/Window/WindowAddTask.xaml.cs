using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public partial class WindowAddTask : MetroWindow
    {
        public WindowAddTask()
        {
            InitializeComponent();
            IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
            Win11Style.LoadWin11Style(hWnd);
        }

        public static void Show(MetroWindow owner)
        {
            WindowAddTask dialog = new WindowAddTask();
            dialog.Owner = owner;
            dialog.ShowDialog();
        }



        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmButton.IsEnabled = false;
            //TODO 支持选择设备和磁盘？？
            //WkyAccountManager.WkyApi.CreateTaskWithUrlResolve();

            try
            {
                var result = await WkyApiManager.Instance.DownloadUrl(UrlTextBox.Text);
                if (result)
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await this.ShowMessageAsync("添加异常，请重试", ex.ToString());
            }
            ConfirmButton.IsEnabled = true;
        }




        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void UrlTextBox_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Note that you can have more than one file.
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var file = files[0];
                //HandleFile(file);

                //判断是不是BT
                if (!file.EndsWith(".torrent"))
                {
                    return;
                }

                try
                {
                    var result = await WkyApiManager.Instance.DownloadBtFile(file);
                    if (result)
                    {

                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    await this.ShowMessageAsync("添加异常，请重试", ex.ToString());
                }
                
            }
            else if (e.Data.GetDataPresent(DataFormats.Text))
            {
                //粘贴上去？
            }
        }

        private void UrlTextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }
    }
}
