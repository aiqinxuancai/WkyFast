
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

namespace WkyFast.Dialogs
{
    /// <summary>
    /// WindowAddTask.xaml 的交互逻辑
    /// </summary>
    public partial class WindowAddTask : Window
    {
        public WindowAddTask()
        {
            InitializeComponent();
            IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
            Win11Style.LoadWin11Style(hWnd);
        }

        public static void Show(Window owner)
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
            var files = UrlTextBox.Text.Split("\r\n");
            files = files.Where(a => !string.IsNullOrWhiteSpace(a)).ToArray();
            int count = 0;
            foreach (var file in files)
            {
                try
                {
                    var result = await WkyApiManager.Instance.DownloadUrl(file);
                    if (result)
                    {
                        EasyLogManager.Logger.Info($"任务已添加：{file}");
                        count++;
                    }
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine(ex);
                    EasyLogManager.Logger.Error(ex);
                    //await this.ShowMessageAsync("添加异常，请重试", ex.ToString());
                }
            }


            if (count == 0)
            {
                EasyLogManager.Logger.Info($"任务添加失败");
                //await this.ShowMessageAsync("添加失败", $"任务添加失败");
            }
            else if (files.Length != count)
            {
                EasyLogManager.Logger.Info($"成功添加{count}个任务，有{files.Length - count}个添加失败");
                //await this.ShowMessageAsync("部分添加失败", $"成功添加{count}个任务，有{files.Length - count}个添加失败");
            }
            else
            {
                this.Close();
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


                int count = 0;
                foreach (var file in files)
                {
                    //判断是不是BT
                    if (!file.EndsWith(".torrent"))
                    {
                        EasyLogManager.Logger.Error($"任务不是torrent文件：{file}");
                        continue;
                    }

                    try
                    {
                        var result = await WkyApiManager.Instance.DownloadBtFile(file);
                        if (result)
                        {
                            EasyLogManager.Logger.Info($"任务已添加：{file}");
                            count++;
                        }

                    }
                    catch (Exception ex)
                    {
                        EasyLogManager.Logger.Error(ex);
                    }

                }

                if (count == 0)
                {
                    EasyLogManager.Logger.Info($"任务添加失败");
                    //await this.ShowMessageAsync("添加失败", $"任务添加失败");
                }
                else if (files.Length != count)
                {
                    EasyLogManager.Logger.Info($"成功添加{count}个任务，有{files.Length - count}个添加失败");
                    //await this.ShowMessageAsync("部分添加失败", $"成功添加{count}个任务，有{files.Length - count}个添加失败");
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
