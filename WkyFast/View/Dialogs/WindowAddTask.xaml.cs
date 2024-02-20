
using Aliyun.OSS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public partial class WindowAddTask : Window
    {
        public WindowAddTask()
        {
            InitializeComponent();
            IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
            Win11Style.LoadWin11Style(hWnd);
            LoadDefaultPathSelected();
        }


        public static void Show(Window owner)
        {
            WindowAddTask dialog = new WindowAddTask();
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

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (!TextBoxPath.Text.StartsWith("/"))
            {
                MainWindow.Instance.ShowSnackbar("添加失败", $"路径需要用/开头");
                return;
            }

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
                    var result = await Aria2ApiManager.Instance.DownloadUrl(file, TextBoxPath.Text);
                    if (result.SuccessCount > 0)
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
                MainWindow.Instance.ShowSnackbar("失败", "任务添加失败");
            }
            else if (files.Length != count)
            {
                EasyLogManager.Logger.Info($"成功添加{count}个任务，有{files.Length - count}个添加失败");
                MainWindow.Instance.ShowSnackbar("成功", $"成功添加{count}个任务，有{files.Length - count}个添加失败");
            }
            else
            {
                //EasyLogManager.Logger.Info($"成功添加任务");
                MainWindow.Instance.ShowSnackbar("成功", $"{count}个任务已添加");
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
            if (!TextBoxPath.Text.StartsWith("/"))
            {
                MainWindow.Instance.ShowSnackbar("添加失败", $"路径需要用/开头");
                return;
            }

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
                        var result = await Aria2ApiManager.Instance.DownloadBtFile(file, TextBoxPath.Text);
                        if (result.SuccessCount > 0)
                        {
                            EasyLogManager.Logger.Info($"任务已添加：{file}");
                            count ++;
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
                    MainWindow.Instance.ShowSnackbar("失败", "任务添加失败");
                }
                else if (files.Length != count)
                {
                    EasyLogManager.Logger.Info($"成功添加{count}个任务，有{files.Length - count}个添加失败");
                    MainWindow.Instance.ShowSnackbar("成功", $"成功添加{count}个任务，有{files.Length - count}个添加失败");
                }
                else
                {
                    MainWindow.Instance.ShowSnackbar("成功", $"{count}个任务已添加");
                    this.Close();
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


        private void TextBoxPath_TextChanged(object sender, TextChangedEventArgs e)
        {
            //当前选择的设备ID
            AppConfig.Instance.ConfigData.AddTaskSavePathDict[AppConfig.Instance.ConfigData.Aria2Rpc] = TextBoxPath.Text;
            AppConfig.Instance.Save();
        }
    }
}
