using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
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

            this.ViewModel = viewModel;
            this.ViewModel = Aria2ApiManager.Instance.TaskList;

            Aria2ApiManager.Instance.EventReceived
                .OfType<LoginResultEvent>()
                .Subscribe(async r =>
                {
                    if (r.IsSuccess)
                    {
                        this.AddTaskButton.IsEnabled = true;
                    }
                    else
                    {
                        this.AddTaskButton.IsEnabled = false;
                    }
                });

        }

        private List<TaskModel> _selectedItems;


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

        }

        private async void MenuCopyTitle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var title = "";
                foreach (var item in _selectedItems)
                {
                    title += item.SubscriptionName;
                    if (item != _selectedItems.Last())
                    {
                        title += "\n";
                    }
                }
                Clipboard.SetDataObject(title);
                MainWindow.Instance.ShowSnackbar("已复制标题", $"{title}");
            }
            catch (Exception ex)
            {
                EasyLogManager.Logger.Error(ex);
            }
        }

        private async void MenuRestart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var item in _selectedItems)
                {
                    if (item.Data.Status == Aria2ApiManager.KARIA2_STATUS_PAUSED ||
                            item.Data.Status == Aria2ApiManager.KARIA2_STATUS_ERROR)
                    {
                        await Aria2ApiManager.Instance.UnpauseTask(item.Data.Gid);
                    }
                }
                Aria2ApiManager.Instance.UpdateTask();

            }
            catch (Exception ex)
            {
                EasyLogManager.Logger.Error(ex);
            }
        }

        private async void MenuStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                foreach (var item in _selectedItems)
                {
                    if (item.Data.Status != Aria2ApiManager.KARIA2_STATUS_COMPLETE)
                    {
                        await Aria2ApiManager.Instance.PauseTask(item.Data.Gid);
                    }
                }

                Aria2ApiManager.Instance.UpdateTask();

            }
            catch (Exception ex)
            {
                EasyLogManager.Logger.Error(ex);
            }
        }

        private async void MenuDelete_Click(object sender, RoutedEventArgs e)
        {
            var title = "";
            foreach (var item in _selectedItems)
            {
                title += item.SubscriptionName;
                if (item != _selectedItems.Last())
                {
                    title += "\n";
                }
            }

            var content = "";
            if (_selectedItems.Count == 1) 
            {
                content = $"是否确认删除任务：\r\n{title}？";
            }
            else
            {
                content = $"是否确认删除{_selectedItems.Count}个任务？";
            }

            MainWindow.Instance.ShowMessageBox("提示", content, async () => {
                try
                {
                    foreach (var item in _selectedItems)
                    {
                        await Aria2ApiManager.Instance.DeleteFile(item.Data.Gid);
                    }

                    Aria2ApiManager.Instance.UpdateTask();
                    MainDataGrid.Dispatcher.Invoke(() => MainDataGrid.UnselectAll());
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
            var title = "";
            foreach (var item in _selectedItems)
            {
                title += item.SubscriptionName; //TODO
                if (item != _selectedItems.Last())
                {
                    title += "\n";
                }
            }


            var content = "";
            if (_selectedItems.Count == 1)
            {
                content = $"是否确认删除任务及文件：\r\n{title}？";
            }
            else
            {
                content = $"是否确认删除{_selectedItems.Count}个任务及文件？";
            }



            MainWindow.Instance.ShowMessageBox("提示", content, async () => {
                try
                {
                    foreach (var item in _selectedItems)
                    {
                        await Aria2ApiManager.Instance.DeleteFile(item.Data.Gid);
                    }

                    //TODO 更新列表 Aria2ApiManager.Instance.API.UpdateTask();
                    MainDataGrid.Dispatcher.Invoke(() => MainDataGrid.UnselectAll());
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
                var url = "";
                foreach (var item in _selectedItems)
                {
                    foreach (var file in item.Data.Files)
                    { 
                        url += file.Path;
                        url += "\n";
                    }
                }
                Clipboard.SetDataObject(url);
                MainWindow.Instance.ShowSnackbar("已复制链接", $"{url}");

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

        private void MainDataGrid_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var contextMenu = MainDataGrid.ContextMenu;
            contextMenu.Items.Clear();

            var selectedItems = new List<TaskModel>();
            foreach (var item in MainDataGrid.SelectedItems)
            {
                var myItem = item as TaskModel;
                if (myItem != null)
                {
                    selectedItems.Add(myItem);
                }
            }

            _selectedItems = selectedItems;

            contextMenu.Items.Clear();

            if (selectedItems.Count > 0)
            {
                //展示继续下载
                var showRestartMenu = selectedItems.Any(a =>
                {
                    if (a.Data.Status == Aria2ApiManager.KARIA2_STATUS_PAUSED ||
                        a.Data.Status == Aria2ApiManager.KARIA2_STATUS_ERROR)
                    {
                        return true;
                    }
                    return false;
                });


                var showStopMenu = selectedItems.Any(a =>
                {
                    if (a.Data.Status == Aria2ApiManager.KARIA2_STATUS_WAITING ||
                        a.Data.Status == Aria2ApiManager.KARIA2_STATUS_ACTIVE)
                    {
                        return true;
                    }
                    return false;
                });

                if (showRestartMenu)
                {
                    var menuRestart = new MenuItem() { Header = "继续下载" };
                    menuRestart.Click += MenuRestart_Click;
                    contextMenu.Items.Add(menuRestart);
                    contextMenu.Items.Add(new Separator());
                }
                else if (showStopMenu)
                {
                    MenuItem menuStop = new MenuItem() { Header = "暂停" };
                    menuStop.Click += MenuStop_Click;
                    contextMenu.Items.Add(menuStop);
                    contextMenu.Items.Add(new Separator());
                }


                MenuItem menuCopyTitle = new MenuItem() { Header = "复制标题" };
                menuCopyTitle.Click += MenuCopyTitle_Click;
                contextMenu.Items.Add(menuCopyTitle);

                MenuItem menuCopyLink = new MenuItem() { Header = "复制链接" };
                menuCopyLink.Click += MenuCopyLink_Click;
                contextMenu.Items.Add(menuCopyLink);

                MenuItem menuDelete = new MenuItem() { Header = $"删除任务({selectedItems.Count})" };
                menuDelete.Click += MenuDelete_Click;
                contextMenu.Items.Add(menuDelete);

                //MenuItem menuDeleteFile = new MenuItem() { Header = $"删除任务及文件({selectedItems.Count})" };
                //menuDeleteFile.Click += MenuDeleteFile_Click;
                //contextMenu.Items.Add(menuDeleteFile);

                DataGrid row = sender as DataGrid;
                row.ContextMenu = contextMenu;
            }
        }
    }
}
