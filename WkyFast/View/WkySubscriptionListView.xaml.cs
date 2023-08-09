using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WkyFast.Dialogs;
using WkyFast.Service;
using WkyFast.Service.Model;
using WkyFast.Service.Model.SubscriptionModel;

namespace WkyFast.View
{
    /// <summary>
    /// WkyTaskListView.xaml 的交互逻辑
    /// </summary>
    public partial class WkySubscriptionListView : Page
    {
        public WkySubscriptionListView()
                    : this(new ObservableCollection<SubscriptionModel>())
        { }

        public WkySubscriptionListView(ObservableCollection<SubscriptionModel> viewModel)
        {
            InitializeComponent();


            //主动刷新？

            this.ViewModel = viewModel;
            this.ViewModel = SubscriptionManager.Instance.SubscriptionModel; //订阅列表绑定
        }


        private SubscriptionModel _lastSubscriptionModel;

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ObservableCollection<SubscriptionModel>), typeof(WkySubscriptionListView));

        public ObservableCollection<SubscriptionModel> ViewModel
        {
            get { return (ObservableCollection<SubscriptionModel>)GetValue(ViewModelProperty); }
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

                SubscriptionModel model = (SubscriptionModel)((DataGridRow)s).DataContext;
                _lastSubscriptionModel = model;

                menu.Items.Clear();
                if (_lastSubscriptionModel != null)
                {
                    MenuItem menuReDownload = new MenuItem() { Header = "重新下载" };
                    menuReDownload.Click += MenuReDownload_Click; ;

                    MenuItem menuDelete = new MenuItem() { Header = "删除" };
                    menuDelete.Click += MenuDelete_Click;

                    menu.Items.Add(menuReDownload);
                    menu.Items.Add(menuDelete);
                    DataGrid row = sender as DataGrid;


                    row.ContextMenu = menu;
                }

                

            };
        }

        private void MenuReDownload_Click(object sender, RoutedEventArgs e)
        {
            //SubscriptionManager.Instance.SubscriptionModel.Remove(_lastSubscriptionModel);
            //SubscriptionManager.Instance.Save();

            if (!SubscriptionManager.Instance.Subscribing)
            {
                _lastSubscriptionModel.AlreadyAddedDownloadModel = new ObservableCollection<SubscriptionSubTaskModel> { };
                SubscriptionManager.Instance.CheckSubscriptionOne(_lastSubscriptionModel);
            }
            else
            {
                MainWindow.Instance.ShowSnackbar("当前无法操作", $"正在执行订阅中...");
            }

        }

        private void MenuDelete_Click(object sender, RoutedEventArgs e)
        {
            SubscriptionManager.Instance.SubscriptionModel.Remove(_lastSubscriptionModel);
            SubscriptionManager.Instance.Save();
        }

        private void SubscriptionButton_Click(object sender, RoutedEventArgs e)
        {
            if (WkyApiManager.Instance.NowDevice != null)
            {
                WindowAddSubscription.Show(Application.Current.MainWindow);
            }
            else
            {
                MainWindow.Instance.ShowSnackbar("无法添加订阅", $"当前没有选中任何设备");
            }
            
        }
    }
}
