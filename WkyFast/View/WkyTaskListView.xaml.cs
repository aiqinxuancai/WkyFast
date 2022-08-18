using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WkyFast.Dialogs;
using WkyFast.Service;
using WkyFast.Service.Model;

namespace WkyFast.View
{
    /// <summary>
    /// WkyTaskListView.xaml 的交互逻辑
    /// </summary>
    public partial class WkyTaskListView : Page
    {
        public WkyTaskListView()
                    : this(new ObservableCollection<TaskModel>())
        { }

        public WkyTaskListView(ObservableCollection<TaskModel> viewModel)
        {
            InitializeComponent();


            //主动刷新？

            this.ViewModel = viewModel;
            this.ViewModel = WkyApiManager.Instance.TaskList;
            //WkySubscriptionListView.ViewModel = SubscriptionManager.Instance.SubscriptionModel; //订阅列表绑定
            //WkyTaskListView.ViewModel = WkyApiManager.Instance.TaskList; //任务列表绑定
        }

        private WkyApiSharp.Service.Model.RemoteDownloadList.Task _lastMenuTaskData;


        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ObservableCollection<TaskModel>), typeof(WkyTaskListView));

        public ObservableCollection<TaskModel> ViewModel
        {
            get { return (ObservableCollection<TaskModel>)GetValue(ViewModelProperty); }
            set
            {
                SetValue(ViewModelProperty, value);
                if (value != null && value.Count > 0)
                {
                    MainDataGrid.SelectedItem = value.First();
                }
            }
        }

        private void UIElement_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = true;
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = MouseWheelEvent;
                eventArg.Source = sender;
                var parent = ((Control)sender).Parent as UIElement;
                parent?.RaiseEvent(eventArg);
            }
        }


        private void MainDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            ContextMenu menu = new ContextMenu();

            e.Row.MouseRightButtonDown += (s, a) => {
                a.Handled = true;
                TaskModel model = (TaskModel)((DataGridRow)s).DataContext;
                _lastMenuTaskData = model.Data;

                if (_lastMenuTaskData != null)
                {
                    menu.Items.Clear();

                    /*
                        0 => "添加中",
                        8 => "等待中",
                        9 => "已暂停",
                        1 => "下载中",
                        11 => "已完成",
                        14 => "准备添加中",
                     */

                    if (_lastMenuTaskData.State == (int)TaskState.Pause)
                    {
                        MenuItem menuRestart = new MenuItem() { Header = "继续下载" };
                        menuRestart.Click += MenuRestart_Click;
                        menu.Items.Add(menuRestart);
                        menu.Items.Add(new Separator());
                    }
                    else if (_lastMenuTaskData.State == (int)TaskState.Completed)
                    {
                        //已完成 不处理
                    }
                    else
                    {
                        MenuItem menuStop = new MenuItem() { Header = "暂停" };
                        menuStop.Click += MenuStop_Click;
                        menu.Items.Add(menuStop);
                        menu.Items.Add(new Separator());
                    }

                    MenuItem menuCopyLink = new MenuItem() { Header = "复制链接" };
                    menuCopyLink.Click += MenuCopyLink_Click;
                    menu.Items.Add(menuCopyLink);

                    MenuItem menuDelete = new MenuItem() { Header = "删除任务" };
                    menuDelete.Click += MenuDelete_Click;
                    menu.Items.Add(menuDelete);

                    MenuItem menuDeleteFile = new MenuItem() { Header = "删除任务及文件" };
                    menuDeleteFile.Click += MenuDeleteFile_Click;
                    menu.Items.Add(menuDeleteFile);

 

                    DataGrid row = sender as DataGrid;
                    row.ContextMenu = menu;
                }


            };
        }

        private async void MenuRestart_Click(object sender, RoutedEventArgs e)
        {
            WkyApiSharp.Service.Model.RemoteDownloadList.Task t = _lastMenuTaskData;
            try
            {
                bool result = await WkyApiManager.Instance.API.StartTask(WkyApiManager.Instance.NowDevice.PeerId, t.GetOperationCode());
                if (result)
                {
                    WkyApiManager.Instance.API.UpdateTask();
                }
                
            }
            catch (Exception ex)
            {
                EasyLogManager.Logger.Error(ex);
            }
        }

        private async void MenuStop_Click(object sender, RoutedEventArgs e)
        {
            WkyApiSharp.Service.Model.RemoteDownloadList.Task t = _lastMenuTaskData;
            try
            {
                bool result = await WkyApiManager.Instance.API.PauseTask(WkyApiManager.Instance.NowDevice.PeerId, t.GetOperationCode());
                if (result)
                {
                    WkyApiManager.Instance.API.UpdateTask();
                }
                    
            }
            catch (Exception ex)
            {
                EasyLogManager.Logger.Error(ex);
            }
        }

        private async void MenuDelete_Click(object sender, RoutedEventArgs e)
        {
            WkyApiSharp.Service.Model.RemoteDownloadList.Task t = _lastMenuTaskData;
            MainWindow.Instance.ShowMessageBox("提示", $"是否确认删除任务\r\n{t.Name}？", async () => {
                try
                {
                    //status
                    bool result = await WkyApiManager.Instance.API.DeleteTask(WkyApiManager.Instance.NowDevice.PeerId, t.GetOperationCode());
                    if (result)
                    {
                        MainWindow.Instance.ShowSnackbar("成功", $"已删除{t.Name}");
                        WkyApiManager.Instance.API.UpdateTask();
                    } 
                }
                catch (Exception ex)
                {
                    EasyLogManager.Logger.Error(ex);


                }
            }, () => {
                //没有操作
            });

        }

        private async void MenuDeleteFile_Click(object sender, RoutedEventArgs e)
        {
            WkyApiSharp.Service.Model.RemoteDownloadList.Task t = _lastMenuTaskData;

            MainWindow.Instance.ShowMessageBox("提示", $"是否确认删除任务及文件：\r\n{t.Name}？", async () => {
                try
                {
                    bool result = await WkyApiManager.Instance.API.DeleteTask(WkyApiManager.Instance.NowDevice.PeerId, t.GetOperationCode(), true);
                    if (result)
                    {
                        MainWindow.Instance.ShowSnackbar("成功", $"已删除{t.Name}");

                        WkyApiManager.Instance.API.UpdateTask();
                    }
                }
                catch (Exception ex)
                {
                    EasyLogManager.Logger.Error(ex);
                }
            }, () => {
                //没有操作
            });
        }

        private async void MenuCopyLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //_lastMenuTaskData.Url
                WkyApiSharp.Service.Model.RemoteDownloadList.Task t = _lastMenuTaskData;

                Clipboard.SetDataObject(t.Url);

                MainWindow.Instance.ShowSnackbar("已复制链接", $"{t.Url}");
            }
            catch (Exception ex)
            {
                EasyLogManager.Logger.Error(ex);
            }
        }
        private void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            WindowAddTask.Show(Application.Current.MainWindow);
        }
    }
}
