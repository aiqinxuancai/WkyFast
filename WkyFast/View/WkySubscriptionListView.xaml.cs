using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WkyFast.Service;
using WkyFast.Service.Model;

namespace WkyFast.View
{
    /// <summary>
    /// WkyTaskListView.xaml 的交互逻辑
    /// </summary>
    public partial class WkySubscriptionListView : UserControl
    {
        public WkySubscriptionListView()
                    : this(new ObservableCollection<SubscriptionModel>())
        { }

        public WkySubscriptionListView(ObservableCollection<SubscriptionModel> viewModel)
        {
            InitializeComponent();


            //主动刷新？

            this.ViewModel = viewModel;
        }

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
                menu.Items.Clear();
                MenuItem menuDelete = new MenuItem() {  Header = "删除"};
                menuDelete.Click += MenuDelete_Click;
                menu.Items.Add(menuDelete);
                DataGrid row = sender as DataGrid; 
                row.ContextMenu = menu;
            };
        }

        private void MenuDelete_Click(object sender, RoutedEventArgs e)
        {
            //删除直接调用API
            //Get the clicked MenuItem
            var menuItem = (MenuItem)sender;

            //Get the ContextMenu to which the menuItem belongs
            var contextMenu = (ContextMenu)menuItem.Parent;

            //Find the placementTarget
            var item = (DataGrid)contextMenu.PlacementTarget;

            //Get the underlying item, that you cast to your object that is bound
            //to the DataGrid (and has subject and state as property)
            var toDeleteFromBindedList = (SubscriptionModel)item.SelectedCells[0].Item;

            //Remove the toDeleteFromBindedList object from your ObservableCollection
            //yourObservableCollection.Remove(toDeleteFromBindedList);
            SubscriptionManager.Instance.SubscriptionModel.Remove(toDeleteFromBindedList);
            SubscriptionManager.Instance.Save();
        }
    }
}
